using NeeLaboratory.Generators;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NeeLaboratory.ComponentModel
{
    public partial class FlagMonitor<T>
        where T : struct, Enum
    {
        private int _rawFlags;

        static FlagMonitor()
        {
            if (Unsafe.SizeOf<T>() != sizeof(int))
            {
                throw new NotSupportedException($"The type '{typeof(T).Name}' must be a 4-byte (int) enum.");
            }
        }

        [Subscribable]
        public event EventHandler<FlagMonitorChangedEventArgs<T>>? Changed;

        public T Flags => Unsafe.BitCast<int, T>(_rawFlags);

        public void SetFlag(T flag, bool state)
        {
            if (state)
            {
                SetFlag(flag);
            }
            else
            {
                ClearFlag(flag);
            }
        }

        public void SetFlag(T flag)
        {
            int value = Unsafe.BitCast<T, int>(flag);
            SetRawFlags(_rawFlags | value);
        }

        public void ClearFlag(T flag)
        {
            int value = Unsafe.BitCast<T, int>(flag);
            SetRawFlags(_rawFlags & ~value);
        }

        private void SetRawFlags(int newFlags)
        {
            int oldFlags = Interlocked.Exchange(ref _rawFlags, newFlags);
            if (oldFlags != newFlags)
            {
                Changed?.Invoke(this, new FlagMonitorChangedEventArgs<T>(oldFlags, newFlags));
            }
        }
    }

    public class FlagMonitorChangedEventArgs<T> : EventArgs
    where T : struct, Enum
    {
        static FlagMonitorChangedEventArgs()
        {
            if (Unsafe.SizeOf<T>() != sizeof(int))
            {
                throw new NotSupportedException($"The type '{typeof(T).Name}' must be a 4-byte (int) enum.");
            }
        }

        public FlagMonitorChangedEventArgs(int oldFlags, int newFlags)
        {
            OldFlags = Unsafe.BitCast<int, T>(oldFlags);
            NewFlags = Unsafe.BitCast<int, T>(newFlags);
            IsActiveChanged = (oldFlags == 0) != (newFlags == 0);
            IsActive = newFlags != 0;
        }

        public T OldFlags { get; }
        public T NewFlags { get; }
        public bool IsActiveChanged { get; }
        public bool IsActive { get; }
    }
}
