using NeeView.IO;
using NeeView.Media.Imaging;
using NeeView.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public interface IExportOverwritePolicy
    {
        FileWriteSettings Resolve(ExportImageService service, string filename);
    }


    public class InvalidExportOverwritePolicy : IExportOverwritePolicy
    {
        public FileWriteSettings Resolve(ExportImageService service, string filename)
        {
            var path = System.IO.Path.GetFullPath(LoosePath.Combine(service.ExportFolder, filename));
            return new(path, false);
        }
    }


    public class ConfirmExportOverwritePolicy : IExportOverwritePolicy
    {
        public FileWriteSettings Resolve(ExportImageService service, string filename)
        {
            var path = System.IO.Path.GetFullPath(LoosePath.Combine(service.ExportFolder, filename));

            if (!File.Exists(path))
            {
                return new(path, false);
            }

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock() { Text = TextResources.GetFormatString("ConfirmFileReplaceDialog.Message", LoosePath.GetFileName(path)), Margin = new Thickness(0, 10, 0, 10) });
            stackPanel.Children.Add(CreateOverwriteContent(path, service));
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
                return new(path, true);
            }
            else if (answer.Command == commandAddNumber)
            {
                return new(FileIO.CreateUniquePath(path), false);
            }
            else
            {
                throw new OperationCanceledException();
            }
        }


        private static FrameworkElement CreateOverwriteContent(string path, ExportImageService exporter)
        {
            PreviewContent content0;
            var image0 = exporter.CreateImageSource();
            content0 = new PreviewContent(image0, exporter.GetLastWriteTime(), exporter.GetLength(path), image0?.GetPixelWidth() ?? 0, image0?.GetPixelHeight() ?? 0);

            PreviewContent content1;
            var fileInfo = new FileInfo(path);
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                content1 = new PreviewContent(bitmap, fileInfo.GetSafeLastWriteTime(), fileInfo.Length, bitmap.PixelWidth, bitmap.PixelHeight);
            }
            catch
            {
                // Show file type icon if not open as image
                var bitmapSourceCollection = FileIconCollection.Current.CreateFileIcon(path, FileIconType.FileType, true, true);
                bitmapSourceCollection.Freeze();
                var bitmap = bitmapSourceCollection.GetBitmapSource(128.0);
                content1 = new PreviewContent(bitmap, fileInfo.GetSafeLastWriteTime(), fileInfo.Length, -1, -1);
            }

            var content = new ExportOverwriteContent(content0, content1);

            return content;
        }
    }


    public class AddNumberExportOverwritePolicy : IExportOverwritePolicy
    {
        public FileWriteSettings Resolve(ExportImageService service, string filename)
        {
            var path = System.IO.Path.GetFullPath(LoosePath.Combine(service.ExportFolder, filename));

            if (!File.Exists(path))
            {
                return new(path, false);
            }

            return new(FileIO.CreateUniquePath(path), false);
        }
    }

    public record FileWriteSettings(string FilePath, bool AllowOverwrite);
}