using System;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 起動時のプロセス間セマフォ
    /// </summary>
    public static class BootProcessLock
    {
        private static readonly ProcessLockCore _lock;

        static BootProcessLock()
        {
            _lock = new ProcessLockCore("NeeView.boot", -1);
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
