using NeeView.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public string? Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(FileIconImage), new PropertyMetadata(null, PathProperty_Changed));

        private static void PathProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileIconImage control)
            {
                control.UpdateBitmapSourceCollection();
                control.UpdateImage();
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

        private void UpdateBitmapSourceCollection()
        {
            var path = Path;
            if (path is null)
            {
                _collection = new();
                return;
            }

            var iconType = FileIconType.File;
            if (_regexDrive.IsMatch(path))
            {
                iconType = FileIconType.Drive;
            }
            else if (Directory.Exists(path))
            {
                iconType = FileIconType.Directory;
            }

            _collection = FileIconCollection.Current.CreateFileIcon(path, iconType, true, true);
        }

        private void UpdateImage()
        {
            this.Source = _collection.GetImageSource(IconSize * _scale);
        }

    }

}
