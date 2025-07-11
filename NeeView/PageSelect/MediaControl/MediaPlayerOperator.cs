using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Threading;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer操作
    /// </summary>
    public class MediaPlayerOperator : BindableBase, IDisposable
    {
        // TODO: ブックとページの MediaPlayerOperator インスタンスアクセス方法が特殊すぎるので整備せよ

        /// <summary>
        /// ブックの現在有効なMediaPlayerOperator。
        /// シングルトンではない。
        /// </summary>
        public static MediaPlayerOperator? BookMediaOperator { get; set; }

        /// <summary>
        /// ページの現在有効なMediaPlayerOperator
        /// </summary>
        public static MediaPlayerOperator? PageMediaOperator { get; set; }

        /// <summary>
        /// どちらか有効なオペレーターを返す
        /// </summary>
        public static MediaPlayerOperator? CurrentMediaOperator => BookMediaOperator ?? PageMediaOperator;



        private readonly ViewContentMediaPlayer _player;
        private readonly DispatcherTimer _timer;
        private bool _isTimeLeftDisplay;
        private Duration _duration;
        private TimeSpan _durationTimeSpan = TimeSpan.FromMilliseconds(1.0);
        private double _position;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue = false;
        private readonly DelayAction _delayResume = new();


        public MediaPlayerOperator(ViewContentMediaPlayer player)
        {
            _player = player;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.Duration),
                (s, e) => Duration = _player.Duration));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.HasAudio),
                (s, e) => RaisePropertyChanged(nameof(HasAudio))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.HasVideo),
                (s, e) => RaisePropertyChanged(nameof(HasVideo))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.IsPlaying),
                (s, e) => RaisePropertyChanged(nameof(IsPlaying))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.IsMuted),
                (s, e) => RaisePropertyChanged(nameof(IsMuted))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.Volume),
                (s, e) => RaisePropertyChanged(nameof(Volume))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.IsRepeat),
                (s, e) => RaisePropertyChanged(nameof(IsRepeat))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.ScrubbingEnabled),
                (s, e) => RaisePropertyChanged(nameof(ScrubbingEnabled))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.AudioTracks),
                (s, e) => RaisePropertyChanged(nameof(AudioTracks))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.Subtitles),
                (s, e) => RaisePropertyChanged(nameof(SubtitleTracks))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.Rate),
                (s, e) => RaisePropertyChanged(nameof(Rate))));

            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += DispatcherTimer_Tick;
            _timer.Start();
        }


        /// <summary>
        /// 再生が終端に達したときのイベント
        /// </summary>
        public event EventHandler? MediaEnded;


        public IMediaPlayer Player => _player;

        public Duration Duration
        {
            get { return _duration; }
            set
            {
                if (_disposedValue) return;
                if (_duration != value)
                {
                    _duration = value;
                    _durationTimeSpan = MathUtility.Max(_duration.HasTimeSpan ? _duration.TimeSpan : TimeSpan.Zero, TimeSpan.FromMilliseconds(1.0));
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DurationHasTimeSpan));
                }
            }
        }

        public bool DurationHasTimeSpan
        {
            get { return _duration.HasTimeSpan; }
        }

        public bool HasAudio
        {
            get { return _player.HasAudio; }
        }

        public bool HasVideo
        {
            get { return _player.HasVideo; }
        }

        public double Position
        {
            get { return _position; }
            set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _position, MathUtility.Clamp(value, 0.0, 1.0)))
                {
                    _player.Position = _position;
                    RaisePropertyChanged(nameof(Position));
                    RaisePropertyChanged(nameof(DisplayTime));
                }
            }
        }

        // [0..1]
        public double Volume
        {
            get { return _player.Volume; }
            set
            {
                if (_disposedValue) return;
                _player.Volume = value;
            }
        }

        public bool IsTimeLeftDisplay
        {
            get { return _isTimeLeftDisplay; }
            set
            {
                if (_disposedValue) return;
                if (_isTimeLeftDisplay != value)
                {
                    _isTimeLeftDisplay = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DisplayTime));
                }
            }
        }

        public string? DisplayTime
        {
            get
            {
                if (!_duration.HasTimeSpan) return null;

                var total = _durationTimeSpan;
                var now = total.Multiply(Position);
                var left = total - now;

                var totalString = total.GetHours() > 0 ? $"{total.GetHours()}:{total.Minutes:00}:{total.Seconds:00}" : $"{total.Minutes}:{total.Seconds:00}";

                var nowString = _isTimeLeftDisplay
                    ? left.GetHours() > 0 ? $"-{left.GetHours()}:{left.Minutes:00}:{left.Seconds:00}" : $"-{left.Minutes}:{left.Seconds:00}"
                    : now.GetHours() > 0 ? $"{now.GetHours()}:{now.Minutes:00}:{now.Seconds:00}" : $"{now.Minutes}:{now.Seconds:00}";

                return nowString + " / " + totalString;
            }
        }

        public bool IsPlaying
        {
            get => _player.IsPlaying;
            set
            {
                if (_disposedValue) return;
                _player.IsPlaying = value;
            }
        }

        public bool IsRepeat
        {
            get => _player.IsRepeat;
            set
            {
                if (_disposedValue) return;
                _player.IsRepeat = value;
            }
        }

        public bool IsMuted
        {
            get => _player.IsMuted;
            set
            {
                if (_disposedValue) return;
                _player.IsMuted = value;
            }
        }

        public bool ScrubbingEnabled
        {
            get => _player.ScrubbingEnabled;
        }

        public bool IsScrubbing
        {
            get => _player.IsScrubbing;
            set
            {
                if (_disposedValue) return;
                _player.IsScrubbing = value;
            }
        }

        public bool CanControlTracks => _player.CanControlTracks;

        public TrackCollection? AudioTracks
        {
            get => _player.AudioTracks;
        }

        public TrackCollection? SubtitleTracks
        {
            get => _player.Subtitles;
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


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    MediaEnded = null;
                    _disposables.Dispose();
                    _timer.Stop();
                    _player.MediaFailed -= Player_MediaFailed;
                    _player.MediaOpened -= Player_MediaOpened;
                    _player.MediaEnded -= Player_MediaEnded;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void RaisePositionPropertyChanged()
        {
            _position = _player.Position;
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(DisplayTime));
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            Dispose();
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (_disposedValue) return;
            if (!IsRepeat)
            {
                MediaEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
        }


        // 通常用タイマー処理
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (_disposedValue) return;
            if (! _player.IsActive || _player.IsScrubbing) return;

            if (_player.ScrubbingEnabled)
            {
                //Debug.WriteLine($"## Player: {_player.Position}");
                RaisePositionPropertyChanged();
            }
        }

        public void Attach()
        {
            if (_disposedValue) return;
            _player.IsActive = true;
            Duration = _player.Duration;
        }

        public void Play()
        {
            if (_disposedValue) return;

            _player.IsActive = true;
            IsPlaying = true;
        }

        public void Pause()
        {
            if (_disposedValue) return;

            IsPlaying = false;
        }

        public void TogglePlay()
        {
            if (_disposedValue) return;

            if (!IsPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// コマンドによる移動
        /// </summary>
        /// <param name="span"></param>
        /// <returns>終端を超える場合は true</returns>
        public bool AddPosition(TimeSpan span)
        {
            if (_disposedValue) return false;
            if (!_player.ScrubbingEnabled) return false;

            var delta = span.Divide(_durationTimeSpan);

            var t0 = Position;
            var t1 = t0 + delta;

            if (delta < 0.0 && t1 < 0.0&& t0 < 0.01)
            {
                if (IsRepeat)
                {
                    t1 = Math.Max(0.0, 1.0 + t1);
                }
                else
                {
                    return true;
                }
            }
            if (delta > 0.0 && t1 > 1.0)
            {
                if (IsRepeat)
                {
                    t1 = 0.0;
                }
                else
                {
                    return true;
                }
            }

            SetPosition(t1);

            return false;
        }

        public void SetPositionFirst()
        {
            if (_disposedValue) return;

            SetPosition(TimeSpan.Zero);
        }

        public void SetPositionLast()
        {
            if (_disposedValue) return;

            SetPosition(_durationTimeSpan);
        }

        // コマンドによる移動
        private void SetPosition(TimeSpan span)
        {
            SetPosition(span.Divide(_durationTimeSpan));
        }

        // コマンドによる移動[0..1]
        private void SetPosition(double position)
        {
            if (_disposedValue) return;
            if (!_player.ScrubbingEnabled) return;

            _player.SetPauseFlag(MediaPlayerPauseBit.SetPosition);
            this.Position = position;
            _delayResume.Request(() => _player.ResetPauseFlag(MediaPlayerPauseBit.SetPosition), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// 音量増減
        /// </summary>
        /// <param name="delta">増減値</param>
        public void AddVolume(double delta)
        {
            if (_disposedValue) return;

            Volume = MathUtility.Clamp(Volume + delta, 0.0, 1.0);
        }

    }

    public static class TimeSpanExtensions
    {
        public static int GetHours(this TimeSpan timeSpan)
        {
            return Math.Abs(timeSpan.Days * 24 + timeSpan.Hours);
        }
    }



}
