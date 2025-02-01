//#define LOCAL_DEBUG

using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NeeView
{

    public class MemoryPool : IDisposable
    {
        private record MemoryUnit(IMemoryElement Key, long Size) : IHasKey<IMemoryElement>;

        private readonly LinkedDicionary<IMemoryElement, MemoryUnit> _collection = new();
        private bool _disposedValue;
        private readonly System.Threading.Lock _lock = new();


        public long TotalSize { get; private set; }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        _collection.Clear();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Add(IMemoryElement element)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                //Debug.WriteLine($"MemoryPool.Add: {element.Index}: {element.GetMemorySize()} Byte");
                Debug.Assert(element.GetMemorySize() > 0);

                var unit = _collection.Remove(element);
                if (unit is not null)
                {
                    TotalSize -= unit.Size;
                }

                unit = new MemoryUnit(element, element.GetMemorySize());
                _collection.AddLast(element, unit);
                TotalSize += unit.Size;
                AssertTotalSize();
            }
        }


        public void Cleanup(long limitSize)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                int removeCount = 0;

                while (limitSize < TotalSize)
                {
                    // 古いものから削除を試みる。ロックされていたらそこで終了
                    var node = _collection.First;
                    if (node is null) break;

                    if (node.Value.Key.IsMemoryLocked)
                    {
                        break;
                    }
                    else
                    {
                        Remove(node);
                        removeCount++;
                    }
                }

                // [DEV]
                if (removeCount > 0)
                {
                    AssertTotalSize();
                }
            }
        }

        private void Remove(LinkedListNode<MemoryUnit> node)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                _collection.Remove(node);
                TotalSize -= node.Value.Size;
                AssertTotalSize();

                node.Value.Key.Unload();
                Trace($"Remove: {node.Value.Key.Index}");
            }
        }

        [Conditional("DEBUG")]
        private void AssertTotalSize()
        {
            lock (_lock)
            {
                var totalSize = _collection.Select(e => e.Size).Sum();
                Debug.Assert(totalSize == TotalSize);
            }
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(CultureInfo.InvariantCulture, s, args)}");
        }
    }

}
