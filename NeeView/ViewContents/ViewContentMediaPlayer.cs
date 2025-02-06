using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Windows;


namespace NeeView
{
    /// <summary>
    /// MediaContext に依存した MediaPlayer
    /// </summary>
    [NotifyPropertyChanged]
    public partial class ViewContentMediaPlayer : IMediaPlayer, INotifyPropertyChanged, IDisposable
    {
        private readonly IMediaContext _mediaContext;
        private readonly IMediaPlayer _player;
        private readonly PageFrameActivity _activity;
        private readonly int _elementIndex;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private bool _isScrubbing;
        private MediaPlayerPauseBit _pauseBits;
        private readonly System.Threading.Lock _lock = new();
        private bool _isPlaying;


        public ViewContentMediaPlayer(IMediaContext mediaContext, IMediaPlayer player, PageFrameActivity activity, int elementIndex)
        {
            _mediaContext = mediaContext;
            _player = player;
            _activity = activity;
            _elementIndex = elementIndex;

            _disposables.Add(_mediaContext.SubscribePropertyChanged(nameof(IMediaContext.Volume),
                (s, e) => { RaisePropertyChanged(nameof(Volume)); Update(); }));

            _disposables.Add(_mediaContext.SubscribePropertyChanged(nameof(IMediaContext.IsMuted),
                (s, e) => { RaisePropertyChanged(nameof(IsMuted)); Update(); }));

            _disposables.Add(_mediaContext.SubscribePropertyChanged(nameof(IMediaContext.IsRepeat),
                (s, e) => { RaisePropertyChanged(nameof(IsRepeat)); Update(); }));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.IsEnabled),
                (s, e) => RaisePropertyChanged(nameof(IsEnabled))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.HasAudio),
                (s, e) => RaisePropertyChanged(nameof(HasAudio))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.IsPlaying),
                (s, e) => RaisePropertyChanged(nameof(IsPlaying))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.HasVideo),
                (s, e) => RaisePropertyChanged(nameof(HasVideo))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.ScrubbingEnabled),
                (s, e) => RaisePropertyChanged(nameof(ScrubbingEnabled))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.Duration),
                (s, e) => RaisePropertyChanged(nameof(Duration))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.Position),
                (s, e) => RaisePropertyChanged(nameof(Position))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.AudioTracks),
                (s, e) => RaisePropertyChanged(nameof(AudioTracks))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.Subtitles),
                (s, e) => RaisePropertyChanged(nameof(Subtitles))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(IMediaPlayer.Rate),
                (s, e) => RaisePropertyChanged(nameof(Rate))));

            _disposables.Add(_activity.SubscribePropertyChanged(
                (s, e) => Update()));

            _isPlaying = true;

            Update();
        }


        public event EventHandler? MediaEnded
        {
            add => _player.MediaEnded += value;
            remove => _player.MediaEnded += value;
        }

        public event EventHandler<ExceptionEventArgs>? MediaFailed
        {
            add => _player.MediaFailed += value;
            remove => _player.MediaFailed += value;
        }

        public event EventHandler? MediaOpened
        {
            add => _player.MediaOpened += value;
            remove => _player.MediaOpened += value;
        }

        public event EventHandler? MediaPlayed
        {
            add => _player.MediaPlayed += value;
            remove => _player.MediaPlayed += value;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsActive { get; set; }

        public bool IsEnabled
        {
            get => _player.IsEnabled;
            set
            {
                if (_disposedValue) return;
                _player.IsEnabled = value;
            }
        }

        public bool IsAudioEnabled
        {
            get => _player.IsAudioEnabled;
            set
            {
                if (_disposedValue) return;
                _player.IsAudioEnabled = value;
            }
        }

        public bool HasAudio => _player.HasAudio;

        public bool HasVideo => _player.HasVideo;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _isPlaying, value))
                {
                    UpdatePlaying();
                }
            }
        }

        public bool ScrubbingEnabled
        {
            get => _player.ScrubbingEnabled;
        }

        public bool IsScrubbing
        {
            get { return _isScrubbing; }
            set
            {
                if (_disposedValue) return;
                if (_isScrubbing != value)
                {
                    _isScrubbing = value;
                    if (IsActive)
                    {
                        if (_isScrubbing)
                        {
                            SetPauseFlag(MediaPlayerPauseBit.Scrubbing);
                        }
                        else
                        {
                            ResetPauseFlag(MediaPlayerPauseBit.Scrubbing);
                        }
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public Duration Duration => _player.Duration;

        public double Position
        {
            get => _player.Position;
            set
            {
                if (_disposedValue) return;
                _player.Position = value;
            }
        }

        public bool CanControlTracks => _player.CanControlTracks;

        public TrackCollection? AudioTracks
        {
            get => _player.AudioTracks;
        }

        public TrackCollection? Subtitles
        {
            get => _player.Subtitles;
        }

        public bool IsMuted
        {
            get => _mediaContext.IsMuted;
            set
            {
                if (_disposedValue) return;
                _mediaContext.IsMuted = value;
            }
        }

        public double Volume
        {
            get => _mediaContext.Volume;
            set
            {
                if (_disposedValue) return;
                _mediaContext.Volume = value;
            }
        }

        public bool RateEnabled => _player.RateEnabled;

        public double Rate
        {
            get => _player.Rate;
            set
            {
                if (_disposedValue) return;
                _player.Rate = value;
            }
        }

        public bool IsRepeat
        {
            get => _mediaContext.IsRepeat;
            set
            {
                if (_disposedValue) return;
                _mediaContext.IsRepeat = value;
            }
        }

        public bool IsDisposed => _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Play()
        {
            if (_disposedValue) return;
            _player.Play();
        }

        public void Pause()
        {
            if (_disposedValue) return;
            _player.Pause();
        }

        private void Update()
        {
            if (_disposedValue) return;

            //Debug.WriteLine($"Media.UpdateState: {Page}");
            _player.IsEnabled = _activity.IsVisible;
            _player.IsMuted = _mediaContext.IsMuted;
            _player.Volume = _mediaContext.Volume;
            _player.IsRepeat = _mediaContext.IsRepeat;

            _player.IsAudioEnabled = _activity.IsSelected && _elementIndex == 0;
        }

        public void SetPauseFlag(MediaPlayerPauseBit bit)
        {
            lock (_lock)
            {
                _pauseBits |= bit;
            }
            UpdatePlaying();
        }

        public void ResetPauseFlag(MediaPlayerPauseBit bit)
        {
            lock (_lock)
            {
                _pauseBits &= ~bit;
            }
            UpdatePlaying();
        }

        /// <summary>
        /// 再生状態更新
        /// </summary>
        private void UpdatePlaying()
        {
            if (_isPlaying && _pauseBits == MediaPlayerPauseBit.None)
            {
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }
    }


    [Flags]
    public enum MediaPlayerPauseBit
    {
        None = 0,
        Scrubbing = (1 << 0),
        SetPosition = (1 << 1),
    }
}
