using System;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// プロセス間ミューテックス
    /// </summary>
    public static class ProcessLock
    {
        private static readonly ProcessLockCore _lock;

        static ProcessLock()
        {
            _lock = new ProcessLockCore("NeeView.m001", 1000 * 10);
        }

        public static IDisposable Lock()
        {
            return _lock.Lock();
        }

        public static async ValueTask<IDisposable> LockAsync(int timeout)
        {
            return await _lock.LockAsync(timeout);
        }
    }
}
