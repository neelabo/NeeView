using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class DefaultMediaPlayerCanvas : MediaPlayerCanvas, IDisposable
    {
        private readonly DefaultMediaPlayer _player;
        private readonly DrawingBrush _videoBrush;
        private readonly Rectangle _videoLayer;
        private readonly ImageBrush _imageBlush;
        private readonly Rectangle _imageLayer;
        private readonly AudioCard _audioCard;
        private readonly TextBlock _errorMessageTextBlock;
        private bool _disposedValue;


        public DefaultMediaPlayerCanvas(PageFrameElement element, MediaViewData source, ViewContentSize contentSize, Rect viewbox, DefaultMediaPlayer player)
        {
            Debug.WriteLine($"Create.MediaPlayer: {source.MediaSource}");

            _player = player;
            _player.MediaPlayed += Player_MediaPlayed;
            _player.MediaFailed += Player_MediaFailed;
            _player.PropertyChanged += Player_PropertyChanged;

            var videoDrawing = new VideoDrawing()
            {
                //Rect = new Rect(size),
                Player = _player.Player,
                Rect = new Rect(0, 0, 256, 256), // Stretch.Fill で引き伸ばすので値は適応でも良い
            };

            _videoBrush = new DrawingBrush()
            {
                Drawing = videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = viewbox,
            };

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

            _audioCard = new AudioCard()
            {
                AudioInfo = source.MediaSource.AudioInfo,
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
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

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _player.MediaPlayed -= Player_MediaPlayed;
                    _player.MediaFailed -= Player_MediaFailed;
                    _player.PropertyChanged -= Player_PropertyChanged;
                }
                _disposedValue = true;
            }
            base.Dispose(disposing);
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
            _videoBrush.Viewbox = viewbox;
            _imageBlush.Viewbox = viewbox;
        }

        private void ShowVideo()
        {
            _videoLayer.Visibility = Visibility.Visible;
            _imageLayer.Visibility = Visibility.Collapsed;
        }
    }
}
