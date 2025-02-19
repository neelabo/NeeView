//#define LOCAL_DEBUG

using NeeLaboratory.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;


namespace NeeView
{
    public class ZipArchiveWriterManager
    {
        static ZipArchiveWriterManager() => Current = new ZipArchiveWriterManager();
        public static ZipArchiveWriterManager Current { get; }


        private Dictionary<string, ZipArchiveWriter> _archives = new();
        private readonly System.Threading.Lock _lock = new();


        private ZipArchiveWriterManager()
        {
        }

        public System.Threading.Lock GetLockObject()
        {
            return _lock;
        }

        /// <summary>
        /// ZIPエントリ削除タスクを作る
        /// </summary>
        /// <remarks>
        /// ZIPエントリを削除するタスクを作る。
        /// すでにタスクが動作しているときは削除要求をそのタスクに追加する。
        /// </remarks>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <param name="idents"></param>
        /// <returns>新しく作られたタスク。生成済の場合は null を返す</returns>
        public Task? CreateDeleteTask(string path, Encoding? encoding, IEnumerable<ZipArchiveEntryIdent> idents, AsyncLock asyncLock)
        {
            lock (_lock)
            {
                var archive = AttachArchive(path, encoding);
                return archive.CreateDeleteTask(idents, asyncLock);
            }
        }

        public ZipArchiveWriter AttachArchive(string path, Encoding? encoding)
        {
            lock (_lock)
            {
                if (!_archives.TryGetValue(path, out var archive))
                {
                    Trace($"Attach {path}");
                    archive = new ZipArchiveWriter(this, path, encoding);
                    _archives.Add(path, archive);
                }
                return archive;
            }
        }

        public void DetachArchive(string path)
        {
            lock (_lock)
            {
                Trace($"Detach {path}");
                _archives.Remove(path);
            }
        }

        public bool Contains(string path)
        {
            lock (_lock)
            {
                return _archives.ContainsKey(path);
            }
        }

        public bool Contains(string path, ZipArchiveEntryIdent? ident)
        {
            if (ident is null) return false;

            lock (_lock)
            {
                if (_archives.TryGetValue(path, out var archive))
                {
                    return archive.Contains(ident);
                }
                return false;
            }
        }

        public void Cancel()
        {
            lock (_lock)
            {
                foreach (var archive in _archives.Values)
                {
                    archive.Cancel();
                }
            }
        }


        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s)
        {
            Debug.WriteLine($"{this.GetType().Name}: {s}");
        }
    }
}
