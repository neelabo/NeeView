using System;
using System.Windows;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 遅延実行
    /// コマンドを遅延実行する。遅延中に要求された場合は古いコマンドをキャンセルする。
    /// </summary>
    public class DelayAction : IDisposable
    {
        private const int _defaultDelayMilliseconds = 1000;

        private readonly System.Threading.Lock _lock = new();
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _timer;
        private readonly Action? _defaultAction;
        private readonly TimeSpan _defaultDelay;
        private Action? _action;


        public DelayAction()
            : this(null, TimeSpan.FromMilliseconds(_defaultDelayMilliseconds), Application.Current.Dispatcher)
        {
        }

        public DelayAction(Dispatcher dispatcher)
            : this(null, TimeSpan.FromMilliseconds(_defaultDelayMilliseconds), dispatcher)
        {
        }

        public DelayAction(Action? action, TimeSpan delay)
            : this(action, delay, Application.Current.Dispatcher)
        {
        }

        public DelayAction(Action? action, TimeSpan delay, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);

            _defaultDelay = delay;
            _defaultAction = action;
        }


        /// <summary>
        /// 実行要求
        /// </summary>
        public void Request()
        {
            if (_disposedValue) return;
            if (_defaultAction is null) throw new InvalidOperationException("Default action is not set");

            Request(_defaultAction, _defaultDelay);
        }

        /// <summary>
        /// 実行要求
        /// </summary>
        public void Request(Action action, TimeSpan delay)
        {
            if (_disposedValue) return;

            _timer.Interval = delay;
            _action = action;

            StartTimer();
        }

        /// <summary>
        /// 実行キャンセル
        /// </summary>
        public void Cancel()
        {
            StopTimer();
            _action = null;
        }

        /// <summary>
        /// 遅延されている命令を即時実行する
        /// </summary>
        public void Flush()
        {
            if (_disposedValue) return;
            if (_action is null) return;

            _dispatcher.Invoke(() => FlushCore());
        }

        private void FlushCore()
        {
            if (_disposedValue) return;
            if (_action is null) return;

            if (StopTimer())
            {
                _action.Invoke();
                _action = null;
            }
        }

        private void StartTimer()
        {
            lock (_lock)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private bool StopTimer()
        {
            lock (_lock)
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// timer callback
        /// </summary>
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            FlushCore();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Cancel();
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
