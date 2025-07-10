using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// スライドショー管理
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SlideShow : INotifyPropertyChanged, IDisposable
    {
        static SlideShow() => Current = new SlideShow();
        public static SlideShow Current { get; }

        private readonly Timer _timer;

        private bool _isPlayingSlideShow;
        private bool _isPlayingSlideShowMemento;
        private readonly DisposableCollection _disposables = new();
        private bool _isMoving;

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
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler<SlideShowPlayedEventArgs>? Played;


        /// <summary>
        /// スライドショー再生状態
        /// </summary>
        public bool IsPlayingSlideShow
        {
            get { return _isPlayingSlideShow; }
            set
            {
                if (_disposedValue) return;

                if (_isPlayingSlideShow != value)
                {
                    _isPlayingSlideShow = value;
                    if (_isPlayingSlideShow)
                    {
                        StartTimer();
                    }
                    else
                    {
                        StopTimer();
                    }
                    RaisePropertyChanged();
                }
            }
        }


        private void PageFrameBoxPresenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
        {
            //Debug.WriteLine($"ViewPageChanged: {string.Join(",", e.Pages.Select(e => e.Index.ToString()))}");
            ResetTimer();
        }

        private void SlideShowConfig_SlideShowIntervalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTimerInterval();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlayingSlideShow, _timer.Interval));
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            if (e.Book is null)
            {
                IsPlayingSlideShow = false;
            }
            else
            {
                ResetTimer();
            }
        }

        public void Play()
        {
            IsPlayingSlideShow = true;
        }

        public void Stop()
        {
            IsPlayingSlideShow = false;
        }

        /// <summary>
        /// スライドショー再生/停止切り替え
        /// </summary>
        public void TogglePlayingSlideShow()
        {
            if (_disposedValue) return;

            this.IsPlayingSlideShow = !this.IsPlayingSlideShow;
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void PauseSlideShow()
        {
            if (_disposedValue) return;

            _isPlayingSlideShowMemento = IsPlayingSlideShow;
            IsPlayingSlideShow = false;
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void ResumeSlideShow()
        {
            if (_disposedValue) return;

            IsPlayingSlideShow = _isPlayingSlideShowMemento;
        }

        private void UpdateTimerInterval()
        {
            if (_disposedValue) return;

            _timer.Interval = Math.Max(Config.Current.SlideShow.SlideShowInterval * 1000.0, 1.0);
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void StartTimer()
        {
            if (_disposedValue) return;

            UpdateTimerInterval();
            _timer.Start();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlayingSlideShow, _timer.Interval));
        }

        /// <summary>
        /// 再生停止
        /// </summary>
        private void StopTimer()
        {
            _timer.Stop();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(false, 0.0));
        }

        /// <summary>
        /// スライドショータイマーリセット
        /// </summary>
        public void ResetTimer()
        {
            if (_disposedValue) return;
            if (!_timer.Enabled) return;

            UpdateTimerInterval();
            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlayingSlideShow, _timer.Interval));
        }

        /// <summary>
        /// 再生中のタイマー処理
        /// </summary>
        private void Timer_Tick(object? sender, ElapsedEventArgs e)
        {
            if (_disposedValue) return;

            AppDispatcher.BeginInvoke(() =>
            {
                // ドラッグ中はキャンセル
                if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
                {
                    ResetTimer();
                    return;
                }

                // ページ移動
                try
                {
                    _isMoving = true;
                    BookOperation.Current.Control.MoveNext(sender);
                }
                finally
                {
                    _isMoving = false;
                }
            });

            Played?.Invoke(this, new SlideShowPlayedEventArgs(_isPlayingSlideShow, _timer.Interval));
        }

        /// <summary>
        /// ページ終端挙動
        /// </summary>
        public void PageEndAction()
        {
            if (_isMoving)
            {
                Stop();
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, TextResources.GetString("ToggleSlideShowCommand.Off"));
            }
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
