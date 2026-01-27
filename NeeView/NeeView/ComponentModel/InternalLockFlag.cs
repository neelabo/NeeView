using System.Threading;

namespace NeeView.ComponentModel
{
    /// <summary>
    /// 排他フラグ
    /// </summary>
    public class InternalLockFlag
    {
        private int _isLocked;

        public bool Lock()
        {
            return Interlocked.CompareExchange(ref _isLocked, 1, 0) == 0;
        }

        public void Unlock()
        {
            Interlocked.Exchange(ref _isLocked, 0);
        }

        public bool IsLocked()
        {
            return _isLocked == 1;
        }
    }
}
