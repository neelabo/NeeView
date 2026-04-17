//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 連続実行を抑制する DelayAction
    /// </summary>
    /// <remarks>
    /// 一定時間内に実行できるアクションを１つに限定し、重複する場合は遅延させて最後に要求されたアクションのみ実行する。
    /// </remarks>
    [LocalDebug]
    public partial class ConstDelayAction : IDisposable
    {
        private Action? _action;
        private bool _disposedValue;
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _timer;
        private readonly Lock _lock = new();
        private readonly int _interval;
        private int _timestamp;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="interval">制限期間 (ms)</param>
        public ConstDelayAction(int interval) : this(Application.Current.Dispatcher, interval)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dispatcher">ディスパッチャ</param>
        /// <param name="interval">制限期間 (ms)</param>
        public ConstDelayAction(Dispatcher dispatcher, int interval)
        {
            _dispatcher = dispatcher;
            _interval = interval;

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
        /// <exception cref="ArgumentNullException"></exception>
        public void Request(Action action)
        {
            lock (_lock)
            {
                Cancel();

                _action = action ?? throw new ArgumentNullException(nameof(action));

                var delay = _interval - (System.Environment.TickCount - _timestamp);

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
            return _dispatcher.Invoke(() => FlushCore());
        }

        private bool FlushCore()
        {
            lock (_lock)
            {
                _timer.Stop();

                if (_action is not null)
                {
                    _action.Invoke();
                    _action = null;
                    _timestamp = System.Environment.TickCount;
                    LocalDebug.WriteLine($"TimeStamp={_timestamp}");
                    return true;
                }

                return false;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            FlushCore();
        }

    }
}
