using NeeLaboratory.IO;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// 画像出力の処理フロー
    /// </summary>
    public static class ExportImageProcedure
    {
        public static void Execute(ExportImageCommandParameter parameter)
        {
            var source = ExportImageSource.Create();

            using var exporter = new ExportImage(source);
            exporter.ExportFolder = string.IsNullOrWhiteSpace(parameter.ExportFolder) ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) : parameter.ExportFolder;
            exporter.Mode = parameter.Mode;
            exporter.HasBackground = parameter.HasBackground;
            exporter.IsOriginalSize = parameter.IsOriginalSize;
            exporter.IsDotKeep = parameter.IsDotKeep;
            exporter.QualityLevel = parameter.QualityLevel;

            string filename = exporter.CreateFileName(parameter.FileNameMode, parameter.FileFormat);
            bool isOverwrite;

            if (string.IsNullOrWhiteSpace(parameter.ExportFolder))
            {
                var dialog = new ExportImageSeveFileDialog(exporter.ExportFolder, filename, exporter.Mode == ExportImageMode.View);
                var result = dialog.ShowDialog(MainWindow.Current);
                if (result != true) return;
                filename = dialog.FileName;
                isOverwrite = true;
            }
            else
            {
                filename = LoosePath.Combine(exporter.ExportFolder, filename);
                if (System.IO.Directory.Exists(filename))
                {
                    new MessageDialog($"Directory '{LoosePath.GetFileName(filename)}' already exists.", TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                    return;
                }
                else if (System.IO.File.Exists(filename))
                {
                    switch (parameter.OverwriteMode)
                    {
                        case ExportImageOverwriteMode.Confirm:
                            var dialog = new MessageDialog(TextResources.GetFormatString("ConfirmFileReplaceDialog.Message", LoosePath.GetFileName(filename)), TextResources.GetString("ConfirmFileReplaceDialog.Title"));
                            var commandReplace = new UICommand("@ConfirmFileReplaceDialog.Replace") { IsPossible = true };
                            var commandAddNumber = new UICommand("@ConfirmFileReplaceDialog.AddNumber") { IsPossible = true };
                            dialog.Commands.Add(commandReplace);
                            dialog.Commands.Add(commandAddNumber);
                            dialog.Commands.Add(UICommands.Cancel);
                            var answer = dialog.ShowDialog();
                            if (answer.Command == commandReplace)
                            {
                                isOverwrite = true;
                            }
                            else if (answer.Command == commandAddNumber)
                            {
                                filename = FileIO.CreateUniquePath(filename);
                                isOverwrite = false;
                            }
                            else
                            {
                                return;
                            }
                            break;

                        case ExportImageOverwriteMode.AddNumber:
                            filename = FileIO.CreateUniquePath(filename);
                            isOverwrite = false;
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported overwrite mode: {parameter.OverwriteMode}");
                    }
                }
                else
                {
                    isOverwrite = false;
                }
            }

            exporter.Export(filename, isOverwrite);

            if (parameter.IsShowToast)
            {
                var toast = new Toast(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("ExportImage.Message.Success"), filename));
                ToastService.Current.Show(toast);
            }
        }

    }
}
