using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// テンポラリファイルキャッシュシステム
    /// </summary>
    public class TempFileCache
    {
        static TempFileCache() => Current = new TempFileCache();
        public static TempFileCache Current { get; }


        private readonly Dictionary<string, TempFile> _caches = new();

        private readonly System.Threading.Lock _lock = new();


        /// <summary>
        /// TempFileをキャッシュから取得。
        /// </summary>
        /// <param name="key">Ident</param>
        /// <returns>キャッシュに存在しない場合はnull</returns>
        public TempFile? Get(string key)
        {
            lock (_lock)
            {
                var tempFile = _caches.ContainsKey(key) ? _caches[key] : null;
                tempFile?.UpdateLastAccessTime();
                ////if (tempFile != null) Debug.WriteLine($"Cache Get: {key}");
                return tempFile;
            }
        }

        /// <summary>
        /// TempFileをキャッシュに登録。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="tempFile"></param>
        public void Add(string key, TempFile tempFile)
        {
            const int capacity = 5;

            if (tempFile == null || tempFile.IsDisposed) return;

            lock (_lock)
            {
                ////Debug.WriteLine($"Cache Add: {key}");
                tempFile.UpdateLastAccessTime();
                _caches[key] = tempFile;

                if (_caches.Count > capacity)
                {
                    var old = _caches.OrderBy(e => e.Value.LastAccessTime).First();

                    ////Debug.WriteLine($"Cache Remove: {old.Key}");
                    _caches.Remove(old.Key);
                }
            }
        }

        /// <summary>
        /// TempFileがキャッシュに存在するかチェック
        /// </summary>
        /// <param name="tempFile"></param>
        /// <returns></returns>
        public bool ContainsValue(TempFile tempFile)
        {
            return _caches.ContainsValue(tempFile);
        }
    }

}
