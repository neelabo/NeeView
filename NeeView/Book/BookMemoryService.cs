//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ブックのメモリ管理
    /// </summary>
    [LocalDebug]
    public partial class BookMemoryService : BindableBase, IBookMemoryService, IDisposable
    {
        private readonly MemoryPool _contentPool = new("BookMemory");
        private bool _disposedValue;

        public BookMemoryService()
        {
        }

        public static long LimitSize => (long)Config.Current.Performance.CacheMemorySize * 1024 * 1024;

        public long TotalSize => _contentPool.TotalSize;

        public bool IsFull => TotalSize >= LimitSize;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _contentPool.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void AddPageContent(IMemoryElement content)
        {
            if (_disposedValue) return;

            _contentPool.Add(content);
            LocalDebug.WriteLine($"AddPageContent: {TotalSize / 1024 / 1024} MB (+{content.MemorySize / 1024:N0} KB)");
            RaisePropertyChanged("");
        }

        public void AddPictureSource(IMemoryElement pictureSource)
        {
            if (_disposedValue) return;

            _contentPool.Add(pictureSource);
            LocalDebug.WriteLine($"AddPictureSource: {TotalSize / 1024 / 1024} MB (+{pictureSource.MemorySize / 1024:N0} KB)");
            RaisePropertyChanged("");
        }

        public void Cleanup(int origin, int direction)
        {
            if (_disposedValue) return;

            _contentPool.Cleanup(LimitSize, new PageDistanceComparer(origin, direction));

            RaisePropertyChanged("");
        }

        // 削除優先順位用のコンペア
        private class PageDistanceComparer : IComparer<IMemoryOwner>
        {
            private readonly int _origin;
            private readonly int _direction;

            public PageDistanceComparer(int origin, int direction)
            {
                _origin = origin;
                _direction = direction;
            }

            public int Compare(IMemoryOwner? x, IMemoryOwner? y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                var distanceX = (x.Index - _origin) * _direction;
                var distanceY = (y.Index - _origin) * _direction;

                if (x.IsMemoryLocked)
                {
                    if (y.IsMemoryLocked)
                    {
                        return Math.Abs(distanceX) - Math.Abs(distanceY);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (y.IsMemoryLocked)
                {
                    return 1;
                }

                if (distanceX >= 0)
                {
                    if (distanceY >= 0)
                    {
                        return distanceX - distanceY;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    if (distanceY >= 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return distanceY - distanceX;
                    }
                }
            }
        }

        /// <summary>
        /// OutOfMemory 発生時の不活性メモリ開放処理
        /// </summary>
        public void CleanupDeep()
        {
            if (_disposedValue) return;

            _contentPool.Cleanup();
            RaisePropertyChanged("");
        }

    }
}
