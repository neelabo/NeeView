using NeeView.PageFrames;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    public abstract class ImageContentControl : ContentControl, IDisposable, IHasImageSource
    {
        protected record struct TargetElement(FrameworkElement View, FrameworkElement Image);


        private readonly PageFrameElement _element;
        private readonly ImageSource _imageSource;
        private readonly ViewContentSize _contentSize;
        private TargetElement _target;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;


        protected ImageContentControl(PageFrameElement source, ImageSource image, ViewContentSize contentSize, PageBackgroundSource backgroundSource)
        {
            this.Focusable = false;

            _element = source;
            _imageSource = image;
            _contentSize = contentSize;

            var grid = new Grid();

            // background
            var pictureInfo = source.PageDataSource.PictureInfo;
            if (pictureInfo is not null)
            {
                var hasAlpha = pictureInfo.HasAlpha;
                if (image is BitmapSource bitmapSource)
                {
                    // リサイズフィルターによってフォーマットが切り替わることがあるため、都度取得する
                    hasAlpha = bitmapSource.HasAlpha();
                }
                if (hasAlpha == true)
                {
                    var background = new Rectangle();
                    background.SetBinding(Rectangle.FillProperty, new Binding(nameof(PageBackgroundSource.Brush)) { Source = backgroundSource });
                    background.Margin = new Thickness(1);
                    background.HorizontalAlignment = HorizontalAlignment.Stretch;
                    background.VerticalAlignment = VerticalAlignment.Stretch;
                    grid.Children.Add(background);
                }
            }

            // target image
            _target = CreateTarget(_imageSource, _element.ViewSizeCalculator.GetViewBox());

            grid.Children.Add(_target.View);

            // image scaling mode
            UpdateBitmapScalingMode();

            this.Content = grid;

            _contentSize.SizeChanged += ContentSize_SizeChanged;
        }


        public ImageSource ImageSource => _imageSource;


        /// <summary>
        /// BitmapScaleMode指定。Printerで使用される。
        /// </summary>
        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    UpdateBitmapScalingMode();
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _contentSize.SizeChanged -= ContentSize_SizeChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected abstract TargetElement CreateTarget(ImageSource imageSource, Rect viewbox);

        private void ContentSize_SizeChanged(object? sender, ViewContentSizeChangedEventArgs e)
        {
            UpdateBitmapScalingMode();
        }

        private void UpdateBitmapScalingMode()
        {
            var imageSize = _imageSource is BitmapSource bitmapSource ? new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight) : new Size(_imageSource.Width, _imageSource.Height);
            ViewContentTools.SetBitmapScalingMode(_target.Image, imageSize, _contentSize, _scalingMode);
        }
    }
}
