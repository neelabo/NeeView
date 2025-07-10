using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.Diagnostics;


namespace NeeView
{
    public class ExportDataDialogService : DialogService
    {
        public static Guid DialogGuid { get; } = Guid.Parse("F4CAB185-940D-46EC-87F6-DCC11D4E25AA");

        public ExportDataDialogService()
        {
            this.Register(nameof(SaveExportDataDialog), new SaveExportDataDialog());
            this.Register(nameof(OpenExportDataDialog), new OpenExportDataDialog());
            this.Register(nameof(ImportDialog), new ImportDialog());
        }
    }

    public class SaveExportDataDialogParameter
    {
        public string FileName { get; set; } = "";
    }


    public class SaveExportDataDialog : IShowDialog
    {
        public const string ExportDataFileExtension = ".nvzip";
        public const string ExportDataFileDialogFilter = "NeeView Backup (.nvzip)|*.nvzip";

        public bool? ShowDialog(object parameter)
        {
            var param = (SaveExportDataDialogParameter)parameter;

            var dialog = new SaveFileDialog();
            dialog.ClientGuid = ExportDataDialogService.DialogGuid;
            dialog.DefaultDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            dialog.OverwritePrompt = true;
            dialog.AddExtension = true;
            dialog.FileName = $"NeeView{Environment.DisplayVersionShort}-{DateTime.Now:yyyyMMdd}";
            dialog.DefaultExt = ExportDataFileExtension;
            dialog.Filter = ExportDataFileDialogFilter;
            dialog.Title = TextResources.GetString("ExportDialog.Title");

            var result = dialog.ShowDialog(MainWindow.Current);

            param.FileName = dialog.FileName;

            return result;
        }
    }


    public class OpenExportDataDialogParameter
    {
        public string FileName { get; set; } = "";
    }

    public class OpenExportDataDialog : IShowDialog
    {
        public bool? ShowDialog(object parameter)
        {
            var param = (OpenExportDataDialogParameter)parameter;

            var dialog = new OpenFileDialog();
            dialog.ClientGuid = ExportDataDialogService.DialogGuid;
            dialog.DefaultDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            dialog.AddExtension = true;
            dialog.DefaultExt = SaveExportDataDialog.ExportDataFileExtension;
            dialog.Filter = SaveExportDataDialog.ExportDataFileDialogFilter;
            dialog.Title = TextResources.GetString("ImportDialog.Title");

            var result = dialog.ShowDialog(MainWindow.Current);

            param.FileName = dialog.FileName;

            return result;
        }
    }


    public class ImportDialog : IShowDialog
    {
        public bool? ShowDialog(object parameter)
        {
            var importer = (Importer)parameter;

            var vm = new ImportControlViewModel(importer);
            var dialogContent = new ImportControl(vm);
            var dialog = new MessageDialog(dialogContent, TextResources.GetString("ImportSelectDialog.Title"));
            dialog.Commands.Add(new UICommand("Word.Import") { IsPossible = true });
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog();
            return result.IsPossible;
        }
    }
}
