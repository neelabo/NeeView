//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ZipArchiveWriter
    {
        private readonly ZipArchiveWriterManager _manager;
        private readonly string _path;
        private readonly Encoding? _encoding;
        private readonly List<ZipArchiveEntryIdent> _idents = new();
        private object _lock;
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

        public Task? CreateDeleteTask(IEnumerable<ZipArchiveEntryIdent> idents)
        {
            lock (_lock)
            {
                foreach (var ident in idents)
                {
                    Trace($"Add: {ident.FullName}");
                }

                _idents.AddRange(idents);
                if (_task is null)
                {
                    _task = Task.Run(async () => await DeleteAsync(_tokenSource.Token));
                    return _task;
                }
            }

            return null;
        }

        private async Task DeleteAsync(CancellationToken token)
        {
            Trace($"Start task: {_path}");

            var tempFilename = FileIO.CreateUniquePath(_path + ".temp");

            var cancelableObject = new CancelableObject(_path, () => _tokenSource.Cancel());
            WorkingProgressWatcher.Current.Add(cancelableObject);

            try
            {
                using var processLock = MainViewComponent.Current.LockProcessing();
                var index = 0;

                while (true)
                {
                    Trace($"Copy to temp file: {_path}");

                    token.ThrowIfCancellationRequested();
                    await CopyAsync(_path, tempFilename, token);

                    while (true)
                    {
                        Trace($"Open archive: {_path}");

                        token.ThrowIfCancellationRequested();
                        using (var archive = ZipFile.Open(tempFilename, ZipArchiveMode.Update, _encoding))
                        {
                            archive.Hotfix();

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
                                    Trace($"Cannot found {ident.FullName}");
                                }
                                else
                                {
                                    Trace($"Delete entry: {entry.FullName}");
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
                    Trace($"Replace file: {_path}");

                    // 元のファイルへ差し替え。
                    // 元ファイルアクセス中は置き換えできないのでリトライさせる
                    int retryCount = 0;
                    while (true)
                    {
                        try
                        {
                            File.Replace(tempFilename, _path, null);
                            break;
                        }
                        catch
                        {
                            retryCount++;
                            if (retryCount >= 5) throw;
                            Trace($"Retry: {_path}");
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

                    Trace($"Done: {_path}");
                }
            }
            catch (Exception ex)
            {
                Trace($"Exception: {ex.Message}");

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

        private static async Task CopyAsync(string src, string dst, CancellationToken token)
        {
            using var sourceStream = File.Open(src, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destinationStream = File.Create(dst);
            await sourceStream.CopyToAsync(destinationStream, token);
            await destinationStream.FlushAsync(token);
        }


        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s)
        {
            Debug.WriteLine($"{this.GetType().Name}: {s}");
        }
    }
}
