using System;
using System.Threading;
using System.Windows.Threading;

namespace NeeView.Threading
{
    public class IntervalAction : IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _timer;
        private readonly Action _action;
        private int _requestCount;


        public IntervalAction(Action action, TimeSpan interval) : this(action, interval, App.Current.Dispatcher)
        {
        }

        public IntervalAction(Action action, TimeSpan interval, Dispatcher dispatcher)
        {
            _action = action;
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Interval = interval;
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _timer.Start();
        }


        /// <summary>
        /// 実行要求
        /// </summary>
        public void Request()
        {
            if (_disposedValue) return;

            Interlocked.Exchange(ref _requestCount, 1);
        }

        /// <summary>
        /// 実行キャンセル
        /// </summary>
        public void Cancel()
        {
            Interlocked.Exchange(ref _requestCount, 0);
        }

        /// <summary>
        /// 遅延されている命令を即時実行する
        /// </summary>
        public void Flush()
        {
            if (_disposedValue) return;

            _dispatcher.Invoke(() => FlushCore());
        }

        private void FlushCore()
        {
            if (_disposedValue) return;

            if (Interlocked.Exchange(ref _requestCount, 0) > 0)
            {
                _action.Invoke();
            }
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
