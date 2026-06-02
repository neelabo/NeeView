//#define LOCAL_DEBUG

using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// スライドショー管理
    /// </summary>
    [LocalDebug]
    public partial class SlideShow : ObservableObject, IDisposable
    {
        static SlideShow() => Current = new SlideShow();
        public static SlideShow Current { get; }

        private readonly Timer _timer;

        private bool _isPlaying;
        private bool _isPause;
        private readonly DisposableCollection _disposables = new();
        private int _startTickCount;
        private int _count;
        private MediaPlayerOperator? _mediaPlayer;
        private readonly System.Threading.Lock _lock = new();


        private SlideShow()
        {
            // timer for slideshow
            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Tick;

            _disposables.Add(Config.Current.SlideShow.SubscribePropertyChanged(nameof(SlideShowConfig.SlideShowInterval), SlideShowConfig_SlideShowIntervalPropertyChanged));

            _disposables.Add(BookOperation.Current.SubscribeBookChanged(BookOperation_BookChanged));

            _disposables.Add(PageFrameBoxPresenter.Current.SubscribeViewPageChanged(PageFrameBoxPresenter_ViewPageChanged));

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        [Subscribable]
        public event EventHandler<SlideShowPlayedEventArgs>? Played;


        /// <summary>
        /// スライドショー再生状態
        /// </summary>
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_disposedValue) return;

                if (value)
                {
                    _isPause = false;
                }

                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    if (_isPlaying)
                    {
                        StartTimer();
                    }
                    else
                    {
                        StopTimer();
                    }
                    OnPropertyChanged();
                }
            }
        }

        public double Interval => _isPlaying ? _timer.Interval : Config.Current.SlideShow.SlideShowInterval * 1000.0;


        private void SlideShowConfig_SlideShowIntervalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!_isPlaying) return;

            ResetTimer();
        }

        private void PageFrameBoxPresenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
        {
            if (!_isPlaying) return;

            if (!Config.Current.SlideShow.IsPrioritizeTime)
            {
                LocalDebug.WriteLine($"ViewPageChanged: {string.Join(",", e.Pages.Select(e => e.Index.ToString()))}");
                ResetTimer();
            }
            else
            {
                Debug.Assert(_mediaPlayer is null);
            }
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            if (!_isPlaying) return;

            if (e.Book is null)
            {
                Stop();
            }
            else if (!Config.Current.SlideShow.IsPrioritizeTime)
            {
                ResetTimer();
            }
        }

        public void SetPlaying(bool isPlaying)
        {
            IsPlaying = isPlaying;
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Suspend()
        {
            if (_disposedValue) return;

            _isPause = true;
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlaying, 0.0));
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void Resume()
        {
            if (_disposedValue) return;

            _isPause = false;
        }

        private void UpdateTimerInterval()
        {
            if (_disposedValue) return;

            var interval = Config.Current.SlideShow.SlideShowInterval * 1000.0;
            if (Config.Current.SlideShow.IsPrioritizeTime)
            {
                var nextTickCount = _startTickCount + _count * interval;
                _timer.Interval = Math.Max(nextTickCount - System.Environment.TickCount, 1.0);
            }
            else
            {
                _timer.Interval = Math.Max(interval, 1.0);
            }

            LocalDebug.WriteLine($"Count = {_count}, Interval = {_timer.Interval:F1} ms");
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void StartTimer()
        {
            if (_disposedValue) return;

            ResetTimerInner();
            _timer.Start();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlaying, _timer.Interval));
        }

        /// <summary>
        /// 再生停止
        /// </summary>
        private void StopTimer()
        {
            ResetWaitAnimation();

            AppDispatcher.BeginInvoke(CancelScroll);

            _timer.Stop();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(false, 0.0));
        }

        /// <summary>
        /// スライドショータイマーリセット
        /// </summary>
        public void ResetTimer()
        {
            if (_disposedValue) return;

            ResetWaitAnimation();

            if (!_timer.Enabled) return;
            ResetTimerInner();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlaying, _timer.Interval));
        }

        /// <summary>
        /// スライドショータイマーリセット (内部用)
        /// </summary>
        private void ResetTimerInner()
        {
            _startTickCount = System.Environment.TickCount;
            _count = 1;
            UpdateTimerInterval();
        }

        /// <summary>
        /// 再生中のタイマー処理
        /// </summary>
        private void Timer_Tick(object? sender, ElapsedEventArgs e)
        {
            if (_disposedValue) return;

            if (_isPause)
            {
                return;
            }

            if (_mediaPlayer is not null)
            {
                LocalDebug.WriteLine("Wait Media EOF...");
                return;
            }

            var player = MediaPlayerOperator.PageMediaOperator;
            if (player is not null
                && Config.Current.SlideShow.IsWaitAnimation && !Config.Current.SlideShow.IsPrioritizeTime
                && player.IsEndOfStreamCountEnabled && player.EndOfStreamCount == 0)
            {
                SetWaitAnimation(player);
            }
            else
            {
                MoveNext();
            }
        }

        private void SetWaitAnimation(MediaPlayerOperator mediaPlayer)
        {
            ResetWaitAnimation();

            lock (_lock)
            {
                _mediaPlayer = mediaPlayer;
                _mediaPlayer.MediaEndOfStreamReached += MediaPlayer_MediaEndOfStreamReached;
            }
        }

        private void ResetWaitAnimation()
        {
            if (_mediaPlayer is null) return;

            lock (_lock)
            {
                _mediaPlayer.MediaEndOfStreamReached -= MediaPlayer_MediaEndOfStreamReached;
                _mediaPlayer = null;
            }
        }

        private void MediaPlayer_MediaEndOfStreamReached(object? sender, EventArgs e)
        {
            ResetWaitAnimation();
            MoveNext();
        }

        private void MoveNext()
        {
            AppDispatcher.BeginInvoke(() =>
            {
                var command = CommandTable.Current.GetElement(Config.Current.SlideShow.NextPageCommandName);
                if (command == CommandElement.None)
                {
                    Config.Current.SlideShow.ResetNextPageCommandName();
                    command = CommandTable.Current.GetElement(Config.Current.SlideShow.NextPageCommandName);
                }

                if (command.CanExecute(this, CommandArgs.Empty))
                {
                    command.Execute(this, CommandArgs.Empty);
                }
            });

            _count++;
            UpdateTimerInterval();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlaying, _timer.Interval));
        }

        public void CancelScroll()
        {
            if (_disposedValue) return;

            PageFrameBoxPresenter.Current.CancelScroll();
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}
