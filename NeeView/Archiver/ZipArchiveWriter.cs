//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeLaboratory.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [LocalDebug]
    public partial class ZipArchiveWriter
    {
        private readonly ZipArchiveWriterManager _manager;
        private readonly string _path;
        private readonly Encoding? _encoding;
        private readonly List<ZipArchiveEntryIdent> _idents = new();
        private readonly System.Threading.Lock _lock;
        private Task? _task;
        private CancellationTokenSource _tokenSource = new();


        public ZipArchiveWriter(ZipArchiveWriterManager manager, string path, Encoding? encoding = null)
        {
            _manager = manager;
            _lock = _manager.GetLockObject();
            _path = path;
            _encoding = encoding;
        }


        public void Cancel()
        {
            _tokenSource.Cancel();
            _tokenSource = new CancellationTokenSource();
        }

        public bool Contains(ZipArchiveEntryIdent ident)
        {
            lock (_lock)
            {
                return _idents.Contains(ident);
            }
        }

        public Task? CreateDeleteTask(IEnumerable<ZipArchiveEntryIdent> idents, AsyncLock asyncLock)
        {
            lock (_lock)
            {
                foreach (var ident in idents)
                {
                    LocalDebug.WriteLine($"Add: {ident.FullName}");
                }

                _idents.AddRange(idents);
                if (_task is null)
                {
                    _task = Task.Run(async () => await DeleteAsync(asyncLock, _tokenSource.Token));
                    return _task;
                }
            }

            return null;
        }

        private async ValueTask DeleteAsync(AsyncLock asyncLock, CancellationToken token)
        {
            LocalDebug.WriteLine($"Start task: {_path}");

            var tempFilename = FileIO.CreateUniquePath(_path + ".temp");

            var cancelableObject = new CancelableObject(_path, () => _tokenSource.Cancel());
            WorkingProgressWatcher.Current.Add(cancelableObject);

            try
            {
                using var processLock = MainViewComponent.Current.LockProcessing();
                var index = 0;

                while (true)
                {
                    LocalDebug.WriteLine($"Copy to temp file: {_path}");

                    token.ThrowIfCancellationRequested();
                    await CopyAsync(_path, tempFilename, token);

                    while (true)
                    {
                        LocalDebug.WriteLine($"Open archive: {_path}");

                        token.ThrowIfCancellationRequested();
                        using (var archive = ZipFile.Open(tempFilename, ZipArchiveMode.Update, _encoding))
                        {
                            while (true)
                            {
                                ZipArchiveEntryIdent ident;
                                lock (_lock)
                                {
                                    if (_idents.Count <= index) break;
                                    ident = _idents[index++];
                                }
                                token.ThrowIfCancellationRequested();
                                var entry = archive.FindEntry(ident);
                                if (entry is null)
                                {
                                    LocalDebug.WriteLine($"Cannot found {ident.FullName}");
                                }
                                else
                                {
                                    LocalDebug.WriteLine($"Delete entry: {entry.FullName}");
                                    entry.Delete();
                                }
                            }
                        }

                        lock (_lock)
                        {
                            if (_idents.Count <= index) break;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    LocalDebug.WriteLine($"Replace file: {_path}");

                    // 元のファイルへ差し替え。
                    // 元ファイルアクセス中は置き換えできないのでリトライさせる
                    int retryCount = 0;
                    while (true)
                    {
                        try
                        {
                            using (await asyncLock.LockAsync(token))
                            {
                                FileIO.Replace(tempFilename, _path, null);
                            }
                            break;
                        }
                        catch
                        {
                            retryCount++;
                            if (retryCount >= 5) throw;
                            LocalDebug.WriteLine($"Retry: {_path}");
                            await Task.Delay(1000, token);
                        }
                    }

                    lock (_lock)
                    {
                        if (_idents.Count <= index)
                        {
                            _manager.DetachArchive(_path);
                            break;
                        }
                    }

                    LocalDebug.WriteLine($"Done: {_path}");
                }
            }
            catch (Exception ex)
            {
                LocalDebug.WriteLine($"Exception: {ex.Message}");

                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
                _manager.DetachArchive(_path);

                throw;
            }
            finally
            {
                WorkingProgressWatcher.Current.Remove(cancelableObject);
            }
        }

        private static async ValueTask CopyAsync(string src, string dst, CancellationToken token)
        {
            using var sourceStream = File.Open(src, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destinationStream = File.Create(dst);
            await sourceStream.CopyToAsync(destinationStream, token);
            await destinationStream.FlushAsync(token);
        }

    }
}
