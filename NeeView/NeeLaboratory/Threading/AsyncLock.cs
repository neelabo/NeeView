using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading
{
    /// <summary>
    /// async な文脈での lock を提供します．
    /// Lock 開放のために，必ず処理の完了後に LockAsync が生成した IDisposable を Dispose してください．
    /// </summary>
    /// <remarks>
    /// https://www.wipiano.net/articles/20191219-async-lock
    /// </remarks>
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async ValueTask<IDisposable> LockAsync(CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            return new Handler(_semaphore);
        }

        public IDisposable Lock()
        {
            return Lock(CancellationToken.None);
        }

        public IDisposable Lock(CancellationToken token)
        {
            _semaphore.Wait(token);
            return new Handler(_semaphore);
        }

        private sealed class Handler : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _disposed = false;

            public Handler(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _semaphore.Release();
                    _disposed = true;
                }
            }
        }
    }
}
