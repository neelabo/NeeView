using NeeLaboratory.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ProcessLockCore
    {
        private readonly Semaphore _semaphore;
        private readonly int _timeout;

        public ProcessLockCore(string label, int millisecondsTimeout = -1)
        {
            _timeout = millisecondsTimeout;
            _semaphore = new Semaphore(1, 1, label, out bool isCreateNew);
            Debug.WriteLine($"Process Semaphore({label}) isCreateNew: {isCreateNew}");
        }

        public IDisposable Lock()
        {
            if (_semaphore.WaitOne(_timeout) != true)
            {
                throw new TimeoutException("Cannot sync with other NeeViews. There may be a problem with NeeView already running.");
            }

            return new Handler(_semaphore);
        }

        public async ValueTask<IDisposable> LockAsync(int timeout)
        {
            await _semaphore.AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeout));
            return new Handler(_semaphore);
        }


        private sealed class Handler : IDisposable
        {
            private readonly Semaphore _semaphore;
            private bool _disposedValue;

            public Handler(Semaphore semaphore)
            {
                _semaphore = semaphore;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }

                    _semaphore.Release();

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
