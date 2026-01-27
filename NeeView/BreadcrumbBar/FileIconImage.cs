using NeeView.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public partial class FileIconImage : Image
    {
        [GeneratedRegex(@"^[a-zA-Z]:\\?$")]
        private static partial Regex _regexDrive { get; }

        public FileIconImage()
        {
            this.Width = IconSize;
            this.Height = IconSize;
        }

        public QueryPath? Path
        {
            get { return (QueryPath)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(QueryPath), typeof(FileIconImage), new PropertyMetadata(null, PathProperty_Changed));

        private static async void PathProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileIconImage control)
            {
                try
                {
                    await control.UpdateBitmapSourceCollectionAsync(CancellationToken.None);
                    control.UpdateImage();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(double), typeof(FileIconImage), new PropertyMetadata(16.0, IconSizeProperty_Changed));

        private static void IconSizeProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileIconImage control)
            {
                control.Width = (double)e.NewValue;
                control.Height = (double)e.NewValue;
                control.UpdateImage();
            }
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _scale = newDpi.DpiScaleX;
            UpdateImage();
        }

        private double _scale = 1.0;

        private BitmapSourceCollection _collection = new();

        private async ValueTask UpdateBitmapSourceCollectionAsync(CancellationToken token)
        {
            if (Path is null || Path.Scheme != QueryScheme.File || string.IsNullOrEmpty(Path.SimplePath))
            {
                _collection = new();
                return;
            }

            var path = Path.SimplePath;
            var iconType = FileIconType.FileType;
            if (_regexDrive.IsMatch(path))
            {
                iconType = FileIconType.Drive;
            }
            else if (Directory.Exists(path))
            {
                iconType = FileIconType.Directory;
            }
            else if (File.Exists(path))
            {
                iconType = FileIconType.File;
            }
            else
            {
                var archiveEntry = await ArchiveEntryUtility.CreateAsync(path, ArchiveHint.None, false, token);
                iconType = archiveEntry.IsDirectory ? FileIconType.DirectoryType : FileIconType.FileType;
            }

            _collection = FileIconCollection.Current.CreateFileIcon(path, iconType, true, true);
        }

        private void UpdateImage()
        {
            this.Source = _collection.GetImageSource(IconSize * _scale);
        }

    }

}
