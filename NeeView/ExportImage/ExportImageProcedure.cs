using NeeLaboratory.IO;
using NeeView.IO;
using NeeView.Media.Imaging;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            exporter.ThrowIfCannotExport();

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
                    throw new IOException($"Directory '{LoosePath.GetFileName(filename)}' already exists");
                }
                else if (System.IO.File.Exists(filename))
                {
                    switch (parameter.OverwriteMode)
                    {
                        case ExportImageOverwriteMode.Confirm:
                            var stackPanel = new StackPanel();
                            stackPanel.Children.Add(new TextBlock() { Text = TextResources.GetFormatString("ConfirmFileReplaceDialog.Message", LoosePath.GetFileName(filename)), Margin = new Thickness(0, 10, 0, 10) });
                            stackPanel.Children.Add(CreateOverwriteContent(filename, exporter));
                            var dialog = new MessageDialog(stackPanel, TextResources.GetString("ConfirmFileReplaceDialog.Title"));
                            var commandReplace = new UICommand("ConfirmFileReplaceDialog.Replace") { IsPossible = true };
                            var commandAddNumber = new UICommand("ConfirmFileReplaceDialog.AddNumber") { IsPossible = true };
                            dialog.Commands.Add(commandReplace);
                            dialog.Commands.Add(commandAddNumber);
                            dialog.Commands.Add(UICommands.Cancel);
                            dialog.Width = 800;
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

            // 非同期処理
            Task.Run(async () =>
            {
                try
                {
                    await exporter.ExportAsync(filename, isOverwrite, CancellationToken.None);

                    if (parameter.IsShowToast)
                    {
                        var toast = new Toast(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ExportImage.Message.Success"), filename));
                        ToastService.Current.Show(toast);
                    }
                }
                catch (Exception ex)
                {
                    var toast = new Toast(ex.Message, TextResources.GetString("Word.Error"), ToastIcon.Error);
                    ToastService.Current.Show(toast);
                }
            });
        }

        private static FrameworkElement CreateOverwriteContent(string filename, ExportImage exporter)
        {
            PreviewContent content0;
            var image0 = exporter.CreateImageSource();
            content0 = new PreviewContent(image0, exporter.GetLastWriteTime(), exporter.GetLength(filename), image0?.GetPixelWidth() ?? 0, image0?.GetPixelHeight() ?? 0);

            PreviewContent content1;
            var fileInfo = new FileInfo(filename);
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filename, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                content1 = new PreviewContent(bitmap, fileInfo.GetSafeLastWriteTime(), fileInfo.Length, bitmap.PixelWidth, bitmap.PixelHeight);
            }
            catch
            {
                // Show file type icon if not open as image
                var bitmapSourceCollection = FileIconCollection.Current.CreateFileIcon(filename, FileIconType.FileType, true, true);
                bitmapSourceCollection.Freeze();
                var bitmap = bitmapSourceCollection.GetBitmapSource(128.0);
                content1 = new PreviewContent(bitmap, fileInfo.GetSafeLastWriteTime(), fileInfo.Length, -1, -1);
            }

            var content = new ExportOverwriteContent(content0, content1);

            return content;
        }
    }
}
