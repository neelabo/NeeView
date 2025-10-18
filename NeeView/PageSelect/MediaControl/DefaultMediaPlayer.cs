using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class DefaultMediaPlayer : IDisposable, INotifyPropertyChanged, IOpenableMediaPlayer
    {
        private static readonly ObjectPool<MediaPlayer> _mediaPlayerPool = new();

        private readonly MediaPlayer _player;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;
        private TimeSpan _startDelay;
        private bool _isPlaying;
        private bool _isEnded;
        private bool _isOpened;
        private bool _isEnabled = true;
        private bool _isAudioEnabled = true;
        private bool _isMuted;
        private bool _isRepeat;


        public DefaultMediaPlayer()
        {
            _player = _mediaPlayerPool.Allocate();
            _player.ScrubbingEnabled = true;

            _disposables.Add(this.SubscribePropertyChanged(nameof(IsEnabled),
                (s, e) => { UpdatePlayed(); UpdateMuted(); }));

            _disposables.Add(this.SubscribePropertyChanged(nameof(IsAudioEnabled),
                (s, e) => UpdateMuted()));

            _disposables.Add(this.SubscribePropertyChanged(nameof(IsMuted),
                (s, e) => UpdateMuted()));

            _disposables.Add(this.SubscribePropertyChanged(nameof(IsRepeat),
                IsRepeat_Changed));
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? MediaPlayed;

        public event EventHandler<ExceptionEventArgs>? MediaFailed;

        public event EventHandler? MediaEnded;

        public event EventHandler? MediaOpened
        {
            add => _player.MediaOpened += value;
            remove => _player.MediaOpened -= value;
        }


        public MediaPlayer Player => _player;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public bool IsAudioEnabled
        {
            get { return _isAudioEnabled; }
            set { SetProperty(ref _isAudioEnabled, value); }
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (_disposedValue) return;
                SetProperty(ref _isMuted, value);
            }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set
            {
                if (_disposedValue) return;
                SetProperty(ref _isRepeat, value);
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                if (_disposedValue) return;
                SetProperty(ref _isPlaying, value);
            }
        }

        public double Volume
        {
            get { return _player.Volume; }
            set
            {
                if (_disposedValue) return;
                _player.Volume = value;
            }
        }

        public bool RateEnabled => true;

        public double Rate
        {
            get { return _player.SpeedRatio; }
            set
            {
                if (_disposedValue) return;
                if (_player.SpeedRatio != value)
                {
                    _player.SpeedRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ScrubbingEnabled
        {
            get { return _player.ScrubbingEnabled; }
        }

        public double Position
        {
            get { return Duration.HasTimeSpan ? _player.Position.Divide(Duration.TimeSpan) : 0.0; }
            set
            {
                if (_disposedValue) return;
                var newPosition = Duration.HasTimeSpan ? Duration.TimeSpan.Multiply(value) : TimeSpan.Zero;
                if (_player.Position != newPosition)
                {
                    _isEnded = false;
                    _player.Position = newPosition;
                }
            }
        }

        public Duration Duration
        {
            get { return _player.NaturalDuration; }
        }

        public bool HasAudio
        {
            get { return _player.HasAudio; }
        }

        public bool HasVideo
        {
            get { return _player.HasVideo; }
        }

        public bool CanControlTracks => false;
        public TrackCollection? AudioTracks => null;
        public TrackCollection? Subtitles => null;

        public int PositionChangeInterval => 500;
        public int ScrubbingInterval => 500;

        public bool IsDisposed => _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _disposables.Dispose();
                    _mediaPlayerPool.Release(_player);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Open(MediaSource mediaSource, TimeSpan delay)
        {
            if (_disposedValue) return;

            if (mediaSource.Path is null) throw new ArgumentException("MediaPlayer requests a Path from mediaSource.");

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            // NOTE: "#"を含むファイルパスがエスケープされると同じパスでの２回目の MediaPlayer がメディアを見つけられなくなるのでエスケープさせない。
            var uri = new Uri(mediaSource.Path, true);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

            _startDelay = delay;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;
            UpdateMuted();

            _player.Open(uri);
            _player.Play();
            _player.IsMuted = true;
            IsPlaying = true;

            // NOTE: MP3等、映像がない場合はOpenedイベントが発生しないため、すぐに再生する。
            if (_player.HasAudio && !_player.HasVideo)
            {
                OnStarted();
            }
        }

        public void Play()
        {
            if (_disposedValue) return;

            IsPlaying = true;
            UpdatePlayed();

            if (_isEnded && _isRepeat)
            {
                Replay();
            }
        }

        public void Stop()
        {
            if (_disposedValue) return;

            IsPlaying = false;
            _player.Stop();
        }

        public void Pause()
        {
            if (_disposedValue) return;

            IsPlaying = false;
            UpdatePlayed();
        }

        private void Replay()
        {
            _isEnded = false;
            _player.Position = TimeSpan.FromMilliseconds(1);
        }

        private void UpdatePlayed()
        {
            var isPlay = _isEnabled && _isPlaying;

            if (isPlay)
            {
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }

        private void UpdateMuted()
        {
            var isMuted = !_isEnabled || !_isAudioEnabled || !_isOpened || _isMuted;
            _player.IsMuted = isMuted;
        }

        private void OnStarted()
        {
            if (_disposedValue) return;

            _isOpened = true;

            UpdatePlayed();
            UpdateMuted();

            RaisePropertyChanged(nameof(HasVideo));
            RaisePropertyChanged(nameof(HasAudio));
            RaisePropertyChanged(nameof(Duration));

            MediaPlayed?.Invoke(this, EventArgs.Empty);
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(HasVideo));
            RaisePropertyChanged(nameof(HasAudio));
            RaisePropertyChanged(nameof(Duration));
            DelayAction(OnStarted, _startDelay);
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            if (_isRepeat)
            {
                Replay();
            }
            else
            {
                _isEnded = true;
                MediaEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Player_MediaFailed(object? sender, System.Windows.Media.ExceptionEventArgs e)
        {
            if (_disposedValue) return;

            Debug.WriteLine($"MediaPlayer.MediaFailed: {e.ErrorException}, DownloadProgress={_player.DownloadProgress}, IsBuffering={_player.IsBuffering}, BufferingProgress={_player.BufferingProgress}");

            MediaFailed?.Invoke(sender, new ExceptionEventArgs(e.ErrorException));
        }

        private void IsRepeat_Changed(object? sender, PropertyChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (_isRepeat && _isEnded && _isPlaying)
            {
                Replay();
            }
        }

        private void Close()
        {
            if (_disposedValue) return;

            _player.MediaOpened -= Player_MediaOpened;
            _player.MediaEnded -= Player_MediaEnded;
            _player.MediaFailed -= Player_MediaFailed;
            _player.Stop();
            _player.Close();
            IsPlaying = false;

            // NOTE: 一瞬黒い画像が表示されるのを防ぐために開放タイミングをずらす。今作では不要か？
#if false
            DelayAction(() =>
            {
                _player.Close();
                _mediaPlayerPool.Release(_player);
            }, 16);
#endif
        }


        // TODO: 汎用化？
        private static void DelayAction(Action action, TimeSpan span)
        {
            AppDispatcher.BeginInvoke(async () =>
            {
                await Task.Delay(span);
                action();
            });
        }

    }
}
