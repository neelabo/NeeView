//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    [LocalDebug]
    public partial class MemoryPool : IDisposable
    {
        private class MemoryUnit : IMemoryElement
        {
            public MemoryUnit(IMemoryOwner owner)
            {
                Owner = owner;
            }

            public List<IMemoryElement> Elements { get; } = new();
            public long Size { get; private set; }
            public IMemoryOwner Owner { get; }
            public long MemorySize => Size;

            public bool Contains(IMemoryElement element)
            {
                return Elements.Contains(element);
            }

            public void Add(IMemoryElement element)
            {
                Debug.Assert(element.Owner == Owner);
                Debug.Assert(!Elements.Contains(element));
                Elements.Add(element);
                Size += element.MemorySize;
            }

            public void Unload()
            {
                foreach (var element in Elements)
                {
                    element.Unload();
                }
            }
        }


        private readonly Dictionary<IMemoryOwner, MemoryUnit> _collection = new();
        private bool _disposedValue;
        private readonly System.Threading.Lock _lock = new();
        private readonly string _name;

        public MemoryPool(string name)
        {
            _name = name;
        }

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

        /// <summary>
        /// MemoryElement を追加する
        /// </summary>
        /// <param name="element"></param>
        public void Add(IMemoryElement element)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                Debug.Assert(element.MemorySize > 0);

                if (!_collection.TryGetValue(element.Owner, out var unit))
                {
                    unit = new MemoryUnit(element.Owner);
                    _collection.Add(unit.Owner, unit);
                }

                if (unit.Contains(element)) return;

                unit.Add(element);
                TotalSize += element.MemorySize;

                AssertTotalSize();

                LocalDebug.WriteLine($"Add: [{element.Owner}] TotalSize {TotalSize / 1024:N0} KB (+{element.MemorySize / 1024:N0} KB)");
            }
        }

        /// <summary>
        /// 指定サイズに収まるようにメモリを削除する
        /// </summary>
        /// <param name="limitSize"></param>
        public void Cleanup(long limitSize, IComparer<IMemoryOwner> comparer)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                LocalDebug.WriteLine($"Cleanup: TotalSize {TotalSize / 1024:N0} KB to {limitSize / 1024:N0} KB");
                if (limitSize >= TotalSize) return;

                // 削除可能順にソート
                var units = _collection.Values.OrderByDescending(e => e.Owner, comparer).ToList();
                LocalDebug.WriteLine($"Sorted: " + string.Join(", ", units.Select(e => e.Owner.Index.ToString())));

                int index = 0;
                while (limitSize < TotalSize)
                {
                    // 古いものから削除を試みる。ロックされていたらそこで終了
                    if (index >= units.Count) break;

                    var unit = units[index];

                    if (unit.Owner.IsMemoryLocked)
                    {
                        break;
                    }
                    else
                    {
                        Remove(unit);
                        index++;
                    }
                }

                // [DEV]
                if (index > 0)
                {
                    AssertTotalSize();
                }
            }
        }

        /// <summary>
        /// メモリ全開放
        /// </summary>
        public void Cleanup()
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                foreach (var unit in _collection.Values)
                {
                    if (!unit.Owner.IsMemoryLocked)
                    {
                        Remove(unit);
                    }
                }

                AssertTotalSize();
            }
        }

        /// <summary>
        /// メモリを削除する
        /// </summary>
        private void Remove(MemoryUnit unit)
        {
            lock (_lock)
            {
                if (_disposedValue) return;

                _collection.Remove(unit.Owner);
                TotalSize -= unit.Size;
                AssertTotalSize();

                unit.Unload();
                LocalDebug.WriteLine($"Remove: [{unit.Owner}] TotalSize {TotalSize / 1024:N0} KB (-{unit.Size / 1024:N0} KB)");
            }
        }

        [Conditional("DEBUG")]
        private void AssertTotalSize()
        {
            lock (_lock)
            {
                var totalSize = _collection.Select(e => e.Value.Size).Sum();
                Debug.Assert(totalSize == TotalSize);
            }
        }

    }

}
