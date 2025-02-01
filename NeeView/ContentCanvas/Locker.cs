using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public class Locker
    {
        public class Key : IDisposable
        {
            private bool _disposedValue = false;

            public Key()
            {
            }

            public Key(Locker locker)
            {
                this.Locker = locker;
            }

            public Locker? Locker { get; set; }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }
                    this.Locker?.Unlock(this);
                    _disposedValue = true;
                }
            }

            ~Key()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }


        private readonly System.Threading.Lock _lock = new();
        private int _lockCount;

        public event EventHandler<LockCountChangedEventArgs>? LockCountChanged;

        public int LockCount => _lockCount;
        public bool IsLocked => _lockCount > 0;

        public Key Lock(bool isLock = true)
        {
            if (!isLock) return new Key();

            int count = -1;
            lock (_lock)
            {
                _lockCount++;
                count = _lockCount;
            }
            LockCountChanged?.Invoke(this, new LockCountChangedEventArgs(count));
            return new Key(this);
        }

        public void Unlock(Key key)
        {
            if (key.Locker == this)
            {
                key.Locker = null;
                int count = -1;
                lock (_lock)
                {
                    if (_lockCount > 0)
                    {
                        _lockCount--;
                        count = _lockCount;
                    }
                }
                if (0 <= count)
                {
                    LockCountChanged?.Invoke(this, new LockCountChangedEventArgs(count));
                }
            }
        }

        public void ForceUnlock()
        {
            lock (_lock)
            {
                _lockCount = 0;
            }
            LockCountChanged?.Invoke(this, new LockCountChangedEventArgs(0));
        }
    }


    public class LockCountChangedEventArgs : EventArgs
    {
        public LockCountChangedEventArgs(int lockCount)
        {
            LockCount = lockCount;
        }

        public int LockCount { get; }

        public bool IsLocked => LockCount > 0;
    }
}
