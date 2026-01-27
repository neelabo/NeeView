#define USE_IMAGEBRUSH

using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Vlc.DotNet.Wpf;

namespace NeeView
{
    public class VlcMediaPlayerCanvas : MediaPlayerCanvas, IDisposable, IHasScalingMode
    {
        private readonly VlcMediaPlayer _player;
#if USE_IMAGEBRUSH
        private readonly ImageBrush _videoBrush;
        private readonly Rectangle _videoLayer;
        private readonly ImageBrush _imageBlush;
        private readonly Rectangle _imageLayer;
#else
        private readonly Image _videoLayer;
        private readonly Image _imageLayer;
#endif
        private readonly AudioCard _audioCard;
        private readonly TextBlock _errorMessageTextBlock;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;
        private PageFrameElement _element;
        private ViewContentSize _contentSize;
        private bool _imageInitialized;

        public VlcMediaPlayerCanvas(PageFrameElement element, MediaViewData source, ViewContentSize contentSize, Rect viewbox, VlcMediaPlayer player)
        {
            Debug.WriteLine($"Create.VlcMediaPlayer: {source.MediaSource}");

            _element = element;
            _contentSize = contentSize;

            _player = player;
            _player.MediaPlayed += Player_MediaPlayed;
            _player.MediaFailed += Player_MediaFailed;
            _player.PropertyChanged += Player_PropertyChanged;

#if USE_IMAGEBRUSH

            _videoBrush = new ImageBrush()
            {
                ImageSource = _player.SourceProvider.VideoSource,
                Stretch = Stretch.Fill,
                Viewbox = viewbox,
            };

            _player.SourceProvider.PropertyChanged += SourceProvider_PropertyChanged;

            _videoLayer = new Rectangle()
            {
                Fill = _videoBrush,
                Visibility = Visibility.Hidden,
            };

            _imageBlush = new ImageBrush()
            {
                ImageSource = source.ImageSource,
                Stretch = Stretch.Fill,
                Viewbox = viewbox,
            };

            _imageLayer = new Rectangle()
            {
                Fill = _imageBlush,
            };


#else
            _videoLayer = new Image()
            {
                Stretch = Stretch.Fill
            };
            RenderOptions.SetBitmapScalingMode(_videoLayer, BitmapScalingMode.Fant);
            _videoLayer.SetBinding(Image.SourceProperty, new Binding(nameof(VlcVideoSourceProvider.VideoSource)) { Source = _player.SourceProvider });

            _imageLayer = new Image()
            {
                Source = source.ImageSource
            };

#endif


            var drawingImage = new DrawingImage()
            {
                Drawing = new GeometryDrawing()
                {
                    Brush = Brushes.Gray,
                    Geometry = App.Current.Resources["g_music"] as PathGeometry,
                },
            };
            drawingImage.Freeze();


            _audioCard = new AudioCard()
            {
                AudioInfo = source.MediaSource.AudioInfo,
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
                //HorizontalAlignment = HorizontalAlignment.Center,
                //MaxWidth = 640.0,
                //Margin = new Thickness(20.0),
            };

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
            this.Background = Brushes.Black;
            this.Children.Add(_videoLayer);
            if (source.ImageSource is not null)
            {
                this.Children.Add(_imageLayer);
            }
            this.Children.Add(_audioCard);
            this.Children.Add(_errorMessageTextBlock);

            UpdateMediaType();

            // image scaling mode
            _contentSize.SizeChanged += ContentSize_SizeChanged;
        }

        private void Player_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(_player.HasVideo))
            {
                AppDispatcher.BeginInvoke(() => UpdateMediaType());
            }
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(_player.HasAudio))
            {
                AppDispatcher.BeginInvoke(() => UpdateMediaType());
            }
        }

        private void UpdateMediaType()
        {
            var isSoundOnly = _player.HasAudio && !_player.HasVideo;
            _audioCard.Visibility = isSoundOnly ? Visibility.Visible : Visibility.Collapsed;
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
                    _player.PropertyChanged -= Player_PropertyChanged;
#if USE_IMAGEBRUSH
                    _player.SourceProvider.PropertyChanged -= SourceProvider_PropertyChanged;
#endif
                }
                _disposedValue = true;
            }
            base.Dispose(disposing);
        }


#if USE_IMAGEBRUSH
        private void SourceProvider_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_disposedValue) return;
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(VlcVideoSourceProvider.VideoSource))
            {
                _videoBrush.ImageSource = _player.SourceProvider.VideoSource;

                if (!_imageInitialized)
                {
                    _imageInitialized = true;
                    UpdateBitmapScalingMode();
                }
            }
        }
#endif

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
#if USE_IMAGEBRUSH
            _videoBrush.Viewbox = viewbox;
            _imageBlush.Viewbox = viewbox;
#endif
            UpdateBitmapScalingMode();
        }

        private void ShowVideo()
        {
            _videoLayer.Visibility = Visibility.Visible;
            _imageLayer.Visibility = Visibility.Collapsed;
        }

        private void UpdateBitmapScalingMode()
        {
            var image = _player.SourceProvider.VideoSource;
            if (image is null) return;

            var imageSize = _videoBrush.ImageSource is BitmapSource bitmapSource ? new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight) : new Size(image.Width, image.Height);

            ViewContentTools.SetBitmapScalingMode(_videoLayer, imageSize, _contentSize, _scalingMode);
        }
    }
}
