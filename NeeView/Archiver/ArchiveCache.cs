using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 生成したArchive を弱参照で保持しておく機構
    /// </summary>
    public class ArchiveCache : IDisposable
    {
        private readonly Dictionary<string, WeakReference<Archive>> _caches = new();
        private readonly System.Threading.Lock _lock = new();


        public List<IDisposable> CollectDisposable()
        {
            lock (_lock)
            {
                return _caches.Values
                       .Select(e => e.TryGetTarget(out var archiver) ? archiver as IDisposable : null)
                       .WhereNotNull()
                       .ToList();
            }
        }

        public void Add(Archive archiver)
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                ////Debug.WriteLine($"ArchiveCache: Add {archiver.SystemPath}");
                _caches[archiver.SystemPath] = new WeakReference<Archive>(archiver);
            }
        }

        public bool TryGetValue(string path, out Archive? archiver)
        {
            if (_disposedValue)
            {
                archiver = null;
                return false;
            }

            if (_caches.Count > 50)
            {
                ////Debug.WriteLine($"ArchiveCache: CleanUp ...");
                CleanUp();
                ////Dump();
            }

            lock (_lock)
            {
                if (_caches.TryGetValue(path, out var weakReference))
                {
                    if (weakReference.TryGetTarget(out archiver))
                    {
                        return true;
                    }
                }
            }

            archiver = null;
            return false;
        }

        /// <summary>
        /// すべてのキャッシュを削除
        /// </summary>
        public void Clear()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                ////Debug.WriteLine($"ArchiveCache: Clear all");
                _caches.Clear();
            }
        }

        /// <summary>
        /// リファレンスが切れた項目を削除
        /// </summary>
        public void CleanUp()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                var removes = _caches.Where(e => !e.Value.TryGetTarget(out var archiver)).Select(e => e.Key).ToList();
                foreach (var key in removes)
                {
                    ////Debug.WriteLine($"ArchiveCache: Remove {key}");
                    _caches.Remove(key);
                }
            }
        }

        /// <summary>
        /// すべてのアーカイブファイルロック解除
        /// </summary>
        public void Unlock()
        {
            if (_disposedValue) return;

            CleanUp();

            lock (_lock)
            {
                foreach (var item in _caches.Values.ToList())
                {
                    if (item.TryGetTarget(out var archiver))
                    {
                        archiver.Unlock();
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            lock (_lock)
            {
                int count = 0;
                foreach (var item in _caches.Values.ToList())
                {
                    if (item.TryGetTarget(out var archiver))
                    {
                        Debug.WriteLine($"ArchiveCache[{count}]: {archiver.SystemPath} => {archiver.ProxyFile?.Path}");
                    }
                    else
                    {
                        Debug.WriteLine($"ArchiveCache[{count}]: removed.");
                    }
                    count++;
                }
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
                if (disposing)
                {
                    // TODO: WeakReference オブジェクトはDisposeする必要あるのか？ -> 不要と判断。経過観察。
#if false
                    var items = CollectDisposable();
                    if (items.Count > 0)
                    {
                        // NOTE: MTAスレッドで実行。SevenZipSharpのCOM例外対策
                        Task.Run(() =>
                        {
                            foreach (var item in items)
                            {
                                item.Dispose();
                            }
                        });
                    }
#endif
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

