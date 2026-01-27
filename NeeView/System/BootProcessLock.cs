using System;

namespace NeeView
{
    /// <summary>
    /// 起動時のプロセス間ミューテックス
    /// </summary>
    /// <remarks>
    /// 起動時のみ使用するため Dispose できるようにしている
    /// </remarks>
    public class BootProcessLock : IDisposable
    {
        private readonly int _timeout;
        private readonly ProcessLockCore _lock;
        private bool _disposedValue;

        public BootProcessLock(int timeout)
        {
            _timeout = timeout;
            _lock = new ProcessLockCore("NeeView.boot", timeout);
        }

        public IDisposable Lock()
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(BootProcessLock));

            try
            {
                return _lock.Lock();
            }
            catch (TimeoutException ex)
            {
                var message = $"NeeView is terminated because it could not be started within {_timeout / 1000} seconds. Check Task Manager and terminate any other NeeView.exe processes that are still running.";
                throw new TimeoutException(message, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _lock.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
