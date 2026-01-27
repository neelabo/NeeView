using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedMediaPlayerCanvas : MediaPlayerCanvas, IDisposable, IHasScalingMode
    {
        private readonly AnimatedMediaPlayer _player;
        private readonly CropControl _cropControl;
        private readonly Image _videoLayer;
        private readonly Image _imageLayer;
        private readonly TextBlock _errorMessageTextBlock;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;
        private PageFrameElement _element;
        private ViewContentSize _contentSize;
        //private bool _imageInitialized;

        public AnimatedMediaPlayerCanvas(PageFrameElement element, MediaViewData source, ViewContentSize contentSize, Rect viewbox, AnimatedMediaPlayer player)
        {
            Debug.WriteLine($"Create.AnimatedMediaPlayer: {source.MediaSource}");

            _element = element;
            _contentSize = contentSize;

            _player = player;
            _player.MediaPlayed += Player_MediaPlayed;
            _player.MediaFailed += Player_MediaFailed;


            var grid = new Grid();

            _videoLayer = _player.Image;
            grid.Children.Add(_videoLayer);

            _imageLayer = new Image()
            {
                Source = source.ImageSource
            };
            if (source.ImageSource is not null)
            {
                grid.Children.Add(_imageLayer);
            }

            _cropControl = new CropControl();
            _cropControl.Target = grid;
            _cropControl.Viewbox = viewbox;

            _errorMessageTextBlock = new TextBlock()
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(40, 20, 40, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
            };

            // root grid
            this.Children.Add(_cropControl);
            this.Children.Add(_errorMessageTextBlock);

            // image scaling mode
            _contentSize.SizeChanged += ContentSize_SizeChanged;
        }


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


        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _contentSize.SizeChanged -= ContentSize_SizeChanged;
                    _player.MediaPlayed -= Player_MediaPlayed;
                    _player.MediaFailed -= Player_MediaFailed;
                }
                _disposedValue = true;
            }
            base.Dispose(disposing);
        }

        private void ContentSize_SizeChanged(object? sender, ViewContentSizeChangedEventArgs e)
        {
            UpdateBitmapScalingMode();
        }

        private void Player_MediaPlayed(object? sender, EventArgs e)
        {
            ShowVideo();
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            if (_errorMessageTextBlock is null) return;

            _videoLayer.Visibility = Visibility.Collapsed;
            _imageLayer.Visibility = Visibility.Collapsed;

            _errorMessageTextBlock.Text = e.ErrorException.Message;
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }

        public override void SetViewbox(Rect viewbox)
        {
            _cropControl.Viewbox = viewbox;
            UpdateBitmapScalingMode();
        }

        private void ShowVideo()
        {
            _videoLayer.Visibility = Visibility.Visible;
            _imageLayer.Visibility = Visibility.Collapsed;

            UpdateBitmapScalingMode();
        }

        private void UpdateBitmapScalingMode()
        {
            var image = _videoLayer;
            if (image is null) return;

            var imageSize = _videoLayer.Source is BitmapSource bitmapSource ? new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight) : new Size(image.Width, image.Height);

            ViewContentTools.SetBitmapScalingMode(_videoLayer, imageSize, _contentSize, _scalingMode);
        }
    }
}
