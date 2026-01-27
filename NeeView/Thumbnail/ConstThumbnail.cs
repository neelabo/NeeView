using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 固定サムネイル.
    /// 画像リソースを外部から指定する
    /// </summary>
    public class ConstThumbnail : IThumbnail, INotifyPropertyChanged
    {
        private readonly ImageSource? _bitmapSource;
        protected Func<ImageSource?>? _create;

        public ConstThumbnail()
        {
        }

        public ConstThumbnail(ImageSource source)
        {
            _bitmapSource = source;
        }

        public ConstThumbnail(Func<ImageSource> crate)
        {
            _create = crate;
        }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        public bool IsValid => true;
        public bool IsUniqueImage => false;
        public bool IsNormalImage => false;
        public Brush Background => Brushes.Transparent;
        public ImageSource? ImageSource => _bitmapSource ?? _create?.Invoke();
    }


    /// <summary>
    /// フォルダーサムネイル
    /// </summary>
    public class FolderThumbnail : ConstThumbnail
    {
        public FolderThumbnail()
        {
            _create = () => FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);
        }
    }


    /// <summary>
    /// リソースサムネイル
    /// </summary>
    public class ResourceThumbnail : ConstThumbnail
    {
        public ResourceThumbnail(string resourceName, FrameworkElement? source = null)
        {
            var resources = source != null ? source.Resources : App.Current.Resources;
            _create = () =>
            {
                return AppDispatcher.Invoke(() => resources[resourceName] as ImageSource);
            };
        }
    }
}
