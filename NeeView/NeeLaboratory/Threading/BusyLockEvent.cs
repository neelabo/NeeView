//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Threading;

namespace NeeLaboratory.Threading
{
    [LocalDebug]
    public partial class BusyLockEvent : IDisposable
    {
        private volatile int _currentCount;
        private bool _disposed;
        private readonly ManualResetEventSlim _event;
        private readonly System.Threading.Lock _lock = new();

        public BusyLockEvent()
        {
            _event = new ManualResetEventSlim(true);
        }

        public int CurrentCount
        {
            get
            {
                int observedCount = _currentCount;
                return observedCount < 0 ? 0 : observedCount;
            }
        }

        public bool IsSet
        {
            get
            {
                return _event.IsSet;
            }
        }

        public WaitHandle WaitHandle
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _event.WaitHandle;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _event.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Increment()
        {
            lock (_lock)
            {
                _currentCount++;
                LocalDebug.WriteLine($"Increment: {_currentCount}");
                if (_currentCount == 1)
                {
                    _event.Reset();
                    LocalDebug.WriteLine($"IsSet: {_event.IsSet}");
                }
            }
        }

        public void Decrement()
        {
            lock (_lock)
            {
                if (_currentCount <= 0)
                {
                    throw new InvalidOperationException("Nothing is locked.");
                }
                _currentCount--;
                LocalDebug.WriteLine($"Decrement: {_currentCount}");
                if (_currentCount == 0)
                {
                    _event.Set();
                    LocalDebug.WriteLine($"IsSet: {_event.IsSet}");
                }
            }
        }

        public void Wait()
        {
            Wait(Timeout.Infinite, CancellationToken.None);
        }

        public void Wait(CancellationToken cancellationToken)
        {
            Wait(Timeout.Infinite, cancellationToken);
        }

        public bool Wait(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            ArgumentOutOfRangeException.ThrowIfLessThan(totalMilliseconds, -1, nameof(timeout));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(totalMilliseconds, int.MaxValue, nameof(timeout));

            return Wait((int)totalMilliseconds, CancellationToken.None);
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            ArgumentOutOfRangeException.ThrowIfLessThan(totalMilliseconds, -1, nameof(timeout));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(totalMilliseconds, int.MaxValue, nameof(timeout));

            return Wait((int)totalMilliseconds, cancellationToken);
        }

        public bool Wait(int millisecondsTimeout)
        {
            return Wait(millisecondsTimeout, CancellationToken.None);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsTimeout, -1);

            ObjectDisposedException.ThrowIf(_disposed, this);
            cancellationToken.ThrowIfCancellationRequested();

            bool returnValue = _event.IsSet;

            if (!returnValue)
            {
                returnValue = _event.Wait(millisecondsTimeout, cancellationToken);
            }

            return returnValue;
        }

        public IDisposable CreateBusyLock()
        {
            return new Handler(this);
        }

        private sealed class Handler : IDisposable
        {
            private readonly BusyLockEvent _event;
            private bool _disposed = false;

            public Handler(BusyLockEvent e)
            {
                _event = e;
                _event.Increment();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _event.Decrement();
                    _disposed = true;
                }
            }
        }

    }
}
