using System;
using System.Windows;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 連続実行を抑制する DelayAction
    /// </summary>
    public class IntervalDelayAction : IDisposable
    {
        private Action? _action;
        private bool _disposedValue;
        private readonly DispatcherTimer _timer;
        private readonly System.Threading.Lock _lock = new();
        private int _timestamp;


        public IntervalDelayAction() : this(Application.Current.Dispatcher)
        {
        }

        public IntervalDelayAction(Dispatcher dispatcher)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Tick += Timer_Tick;

            _timestamp = System.Environment.TickCount;
        }

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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// アクション要求
        /// </summary>
        /// <param name="action">アクション</param>
        /// <param name="delayMilliseconds">遅延時間</param>
        /// <param name="intervalMilliseconds">前回の要求からの実行間隔</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Request(Action action, int delayMilliseconds, int intervalMilliseconds)
        {
            lock (_lock)
            {
                Cancel();

                _action = action ?? throw new ArgumentNullException(nameof(action));

                var now = System.Environment.TickCount;
                var rest = intervalMilliseconds - (now - _timestamp);
                _timestamp = now;

                var delay = Math.Max(delayMilliseconds, rest);
                if (delay <= 0)
                {
                    Flush();
                }
                else
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(delay);
                    _timer.Start();
                }
            }
        }

        public bool Cancel()
        {
            lock (_lock)
            {
                _timer.Stop();

                if (_action is not null)
                {
                    _action = null;
                    return true;
                }

                return false;
            }
        }

        public bool Flush()
        {
            lock (_lock)
            {
                _timer.Stop();

                if (_action is not null)
                {
                    _action.Invoke();
                    _action = null;
                    return true;
                }

                return false;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Flush();
        }

    }
}
