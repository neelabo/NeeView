using NeeView.IO;
using NeeView.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public interface IExportOverwriteResolver
    {
        public bool Exists(string name);
        public string CreateUniqueName(string name);
        public FrameworkElement CreateOverwriteContent(string name, ExportImageService? service);
    }

    public class FileExportOverwriteResolver : IExportOverwriteResolver
    {
        private readonly string _directory;

        public FileExportOverwriteResolver(string directory)
        {
            _directory = directory;
        }

        public string GetFullPath(string name)
        {
            return System.IO.Path.GetFullPath(LoosePath.Combine(_directory, name));
        }

        public bool Exists(string name)
        {
            var path = GetFullPath(name);
            return File.Exists(path);
        }

        public string CreateUniqueName(string name)
        {
            var path = GetFullPath(name);
            var directoryLength = path.Length - name.Length;
            var uniquePath = FileIO.CreateUniquePath(path);
            var uniqueName = uniquePath[directoryLength..];
            return uniqueName;
        }

        public FrameworkElement CreateOverwriteContent(string name, ExportImageService? service)
        {
            if (service is null)
            {
                return new TextBlock() { Text = name, Margin = new Thickness(10) };
            }

            var path = GetFullPath(name);

            PreviewContent content0;
            var image0 = service.CreateImageSource();
            content0 = new PreviewContent(image0, service.GetLastWriteTime(), service.GetLength(path), image0?.GetPixelWidth() ?? 0, image0?.GetPixelHeight() ?? 0);

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


    public class ZipExportOverwriteResolver : IExportOverwriteResolver
    {
        private HashSet<string> _names = new();

        public void Add(string name)
        {
            _names.Add(name);
        }

        public bool Exists(string name)
        {
            return _names.Contains(name);
        }

        public string CreateUniqueName(string name)
        {
            return LoosePath.CreateUniquePath(name, true, Exists);
        }

        public FrameworkElement CreateOverwriteContent(string name, ExportImageService? service)
        {
            return new TextBlock() { Text = "(none)", Margin = new Thickness(10) };
        }
    }
}