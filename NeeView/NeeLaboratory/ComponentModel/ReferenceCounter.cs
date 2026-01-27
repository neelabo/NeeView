using NeeLaboratory.Generators;
using System;
using System.Threading;

namespace NeeLaboratory.ComponentModel
{
    public partial class ReferenceCounter
    {
        private int _count;

        [Subscribable]
        public event EventHandler<ReferenceCounterChangedEventArgs>? Changed;

        public int Count => _count;
        public bool IsActive => 0 < _count;

        public void Increment()
        {
            var count = Interlocked.Increment(ref _count);
            var isActiveChanged = (count == 1);
            Changed?.Invoke(this, new ReferenceCounterChangedEventArgs(count, isActiveChanged));
        }

        public void Decrement()
        {
            var count = Interlocked.Decrement(ref _count);
            var isActiveChanged = (count == 0);
            Changed?.Invoke(this, new ReferenceCounterChangedEventArgs(count, isActiveChanged));
        }
    }


    public class ReferenceCounterChangedEventArgs : EventArgs
    {
        public ReferenceCounterChangedEventArgs(int count, bool isActiveChanged)
        {
            this.Count = count;
            IsActiveChanged = isActiveChanged;
        }

        public int Count { get; }
        public bool IsActiveChanged { get; }
        public bool IsActive => 0 < Count;
    }
}
