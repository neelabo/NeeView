using NeeView.Properties;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class ExportDataPresenter
    {
        static ExportDataPresenter() => Current = new ExportDataPresenter();
        public static ExportDataPresenter Current { get; }

        private ExportDataPresenter()
        {
        }


        private readonly ExportDataDialogService _dialogService = new();


        public void Export(ExportBackupCommandParameter parameter)
        {
            var fileName = parameter.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var dialogParam = new SaveExportDataDialogParameter();
                if (_dialogService.ShowDialog("SaveExportDataDialog", dialogParam) != true) return;
                fileName = dialogParam.FileName;
            }

            try
            {
                SaveDataSync.Current.Flush();
                Exporter.Export(fileName);
            }
            catch (Exception ex)
            {
                new MessageDialog($"{TextResources.GetString("Word.Cause")}: {ex.Message}", TextResources.GetString("ExportErrorDialog.Title")).ShowDialog();
            }
        }


        public void Import(ImportBackupCommandParameter parameter)
        {
            var param = (ImportBackupCommandParameter)parameter.Clone();
            if (string.IsNullOrWhiteSpace(param.FileName) || !System.IO.File.Exists(param.FileName))
            {
                var dialogParam = new OpenExportDataDialogParameter();
                if (_dialogService.ShowDialog("OpenExportDataDialog", dialogParam) != true) return;
                param.FileName = dialogParam.FileName;
            }

            if (string.IsNullOrEmpty(param.FileName)) return;
            using (var importer = new Importer(param))
            {
                if (!param.IsImportActionValid())
                {
                    if (_dialogService.ShowDialog("ImportDialog", importer) != true) return;
                }

                try
                {
                    importer.Import();
                }
                catch (Exception ex)
                {
                    new MessageDialog(CreateExceptionContent(ex), TextResources.GetString("ImportErrorDialog.Title")).ShowDialog();
                }
            }
        }

        private static StackPanel CreateExceptionContent(Exception ex)
        {
            var textBlock = new TextBlock()
            {
                Text = ex.Message,
                TextWrapping = TextWrapping.Wrap
            };

            var textBox = new TextBox()
            {
                Text = ex.ToString(),
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 256,
                Margin = new Thickness(0, 16, 0, 0)
            };

            var panel = new StackPanel();
            panel.Children.Add(textBlock);
            panel.Children.Add(textBox);
            return panel;
        }

    }
}
