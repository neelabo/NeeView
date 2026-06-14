using NeeLaboratory.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ProcessLockCore : IDisposable
    {
        private readonly Mutex _mutex;
        private readonly int _timeout;
        private bool _disposedValue;

        public ProcessLockCore(string label, int millisecondsTimeout = -1)
        {
            _timeout = millisecondsTimeout;
            _mutex = new Mutex(false, label, out bool isCreateNew);
            Debug.WriteLine($"Process Mutex({label}) isCreateNew: {isCreateNew}");
        }

        public int TimeoutMilliseconds => _timeout;

        public IDisposable Lock()
        {
            try
            {
                if (_mutex.WaitOne(_timeout) != true)
                {
                    throw new TimeoutException("Cannot sync with other NeeViews. There may be a problem with NeeView already running.");
                }
            }
            catch (AbandonedMutexException ex)
            {
                // あるスレッドが、解放せずに終了して別のスレッドが破棄した Mutex を取得したときにスローされる例外
                // この例外が発生しても WaitOne は成功している 
                Debug.WriteLine(ex.Message);
            }

            return new Handler(_mutex);
        }

        public async Task<IDisposable> LockAsync(int timeout)
        {
            await _mutex.AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeout));
            return new Handler(_mutex);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _mutex.Dispose();

                _disposedValue = true;
            }
        }

        ~ProcessLockCore()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private sealed class Handler : IDisposable
        {
            private readonly Mutex _mutex;
            private bool _disposedValue;

            public Handler(Mutex mutex)
            {
                _mutex = mutex;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }

                    _mutex.ReleaseMutex();

                    _disposedValue = true;
                }
            }

            ~Handler()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
