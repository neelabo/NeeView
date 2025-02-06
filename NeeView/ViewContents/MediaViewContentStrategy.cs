using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class AnimatedViewContentStrategy : MediaViewContentStrategy
    {
        public AnimatedViewContentStrategy(ViewContent viewContent) : base(viewContent)
        {
        }
    }


    public class MediaViewContentStrategy : IDisposable, IViewContentStrategy, IHasImageSource, IHasViewContentMediaPlayer, IHasScalingMode
    {
        private readonly ViewContent _viewContent;

        private readonly ViewContentMediaPlayer _mediaPlayer;
        private readonly IOpenableMediaPlayer _player;
        private readonly IMediaContext _mediaContext;
        private MediaPlayerCanvas? _playerCanvas;
        private ImageSource? _imageSource;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;


        public MediaViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;

            // メディアブックとメティアページで参照する設定を変える
            _mediaContext = _viewContent.Page.ArchiveEntry.Archive is MediaArchive ? Config.Current.Archive.Media : PageMediaContext.Current;


            _player = AllocateMediaPlayer();
            _mediaPlayer = new ViewContentMediaPlayer(_mediaContext, _player, _viewContent.Activity, _viewContent.ElementIndex);
        }


        public ImageSource? ImageSource => _imageSource;

        public ViewContentMediaPlayer Player => _mediaPlayer;


        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    if (_playerCanvas is IHasScalingMode hasScalingMode)
                    {
                        hasScalingMode.ScalingMode = _scalingMode;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _playerCanvas?.Dispose();
                    _mediaPlayer.Dispose();
                    ReleaseMediaPlayer(_player);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void OnSourceChanged()
        {
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);

            Debug.WriteLine($"Create.MediaPlayer: {_viewContent.ArchiveEntry}");
            var viewData = data as MediaViewData ?? throw new InvalidOperationException();

            _imageSource = viewData.ImageSource;

            var viewbox = _viewContent.Element.ViewSizeCalculator.GetViewBox();

            if (_playerCanvas is not null)
            {
                _playerCanvas.SetViewbox(viewbox);
                return _playerCanvas;
            }

            _playerCanvas = MediaPlayerCanvasFactory.Create(_viewContent.Element, viewData, _viewContent.ViewContentSize, viewbox, _player);
            _player.Open(viewData.MediaSource, TimeSpan.FromSeconds(_mediaContext.MediaStartDelaySeconds));

            return _playerCanvas;
        }


        private IOpenableMediaPlayer AllocateMediaPlayer()
        {
            if (_viewContent.Page.Content is AnimatedPageContent)
            {
                try
                {
                    if (_viewContent.Page.Content.PictureInfo is PictureInfo pictureInfo)
                    {
                        pictureInfo.Decoder = "AnimatedImage";
                    }
                    return new AnimatedMediaPlayer();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Cannot use AnimatedImage.\r\n{ex.Message}", ex);
                }
            }
            else if (Config.Current.Archive.Media.IsLibVlcEnabled)
            {
                try
                {
                    if (_viewContent.Page.Content.PictureInfo is PictureInfo pictureInfo)
                    {
                        pictureInfo.Decoder = "libVLC";
                    }
                    return new VlcMediaPlayer();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Cannot use libVLC.\r\n{ex.Message}", ex);
                }
            }
            else
            {
                try
                {
                    if (_viewContent.Page.Content.PictureInfo is PictureInfo pictureInfo)
                    {
                        pictureInfo.Decoder = "MediaPlayer";
                    }
                    return new DefaultMediaPlayer();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Cannot create Media player.\r\n{ex.Message}", ex);

                }
            }
        }

        private void ReleaseMediaPlayer(IOpenableMediaPlayer player)
        {
            player.Dispose();
        }
    }
}
