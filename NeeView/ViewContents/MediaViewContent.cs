﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace NeeView
{
    public static class MediaPlayerPool
    {
        [Obsolete("TODO: static ダメ！Book切り替えで開放されるように！")]
        public static ObjectPool<MediaPlayer> Default { get; } = new();
    }


    public partial class MediaViewContent : ViewContent
    {
        private ObjectPool<MediaPlayer> _mediaPlayerPool;

        private SimpleMediaPlayer _player;
        private MediaPlayerCanvas? _playerCanvas;
        private DisposableCollection _disposables = new();
        private bool _disposedValue;
        private ImageSource? _imageSource;

        public MediaViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
            : base(element, scale, viewSource, activity)
        {
            _mediaPlayerPool = MediaPlayerPool.Default;

            _player = AllocateMediaPlayer();

            MediaStartDelay = TimeSpan.FromSeconds(Config.Current.Archive.Media.MediaStartDelaySeconds);
        }


        public SimpleMediaPlayer Player => _player;

        public ImageSource? ImageSource => _imageSource;


        public bool HasControl
        {
            get => _player.HasControl;
            set => _player.HasControl = value;
        }

        public TimeSpan MediaStartDelay { get; protected set; }


        private SimpleMediaPlayer AllocateMediaPlayer()
        {
            var player = new SimpleMediaPlayer(_mediaPlayerPool.Allocate());
            player.MediaPlayed += Player_MediaPlayed;
            return player;
        }

        private void ReleaseMediaPlayer(SimpleMediaPlayer player)
        {
            player.MediaPlayed -= Player_MediaPlayed;
            player.Dispose();
            _mediaPlayerPool.Release(player.Player);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _playerCanvas?.Dispose();
                    _playerCanvas = null;

                    ReleaseMediaPlayer(_player);

                    _disposables.Dispose();
                    this.Content = null;
                }
                _disposedValue = true;
            }

            base.Dispose(disposing);
        }


        public override void Initialize()
        {
            base.Initialize();

            _disposables.Add(Activity.SubscribePropertyChanged((s, e) => UpdateVideoStatus()));
        }



        protected override FrameworkElement CreateLoadedContent(Size size, object data)
        {
            var source = data as MediaSource ?? throw new InvalidOperationException();
            return CreateMediaContent(source);
        }


        private FrameworkElement CreateMediaContent(MediaSource source)
        {
            Debug.WriteLine($"Create.MediaPlayer: {ArchiveEntry}");

            _imageSource = source.ImageSource;

            var viewbox = Element.ViewSizeCalculator.GetViewBox();

            if (_playerCanvas is not null)
            {
                _playerCanvas.SetViewbox(viewbox);
                return _playerCanvas;
            }

            _playerCanvas = new MediaPlayerCanvas(source, viewbox, _player);
            _player.Open(new Uri(source.Path), MediaStartDelay);

            return _playerCanvas;
        }

        private void Player_MediaPlayed(object? sender, EventArgs e)
        {
            UpdateVideoStatus();
        }

        private void UpdateVideoStatus()
        {
            _player.IsMuted = !Activity.IsSelected;

            if (Activity.IsVisible)
            {
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }

    }

}
