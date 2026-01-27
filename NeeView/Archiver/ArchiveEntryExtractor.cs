using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryからファイルに展開する。キャンセル可。
    /// </summary>
    public class ArchiveEntryExtractor : IDisposable
    {
        private static TempFileNamePolicy _namePolicy = new(false, "arcv");

        private readonly ArchiveEntry _entry;
        private readonly Task _task;
        private FileProxy? _tempFile;
        private bool _disposedValue;

        // TODO: いきなり処理が始まっているのはよろしくない。実行は別メソッドで。
        public ArchiveEntryExtractor(ArchiveEntry entry)
        {
            _entry = entry;

            _task = Task.Run(async () =>
            {
                _tempFile = await _entry.CreateFileProxyAsync(_namePolicy, false, CancellationToken.None); // ## TODO cancellationToken
            });

            _task.ContinueWith(async (t) =>
            {
                await Task.Delay(1000);
                Expired?.Invoke(this, new(_entry, t.Exception));
            });
        }

        // 期限切れ
        public event EventHandler<ArchiveEntryExtractorExpiredEventArgs>? Expired;


        public async ValueTask<FileProxy> WaitAsync(CancellationToken token)
        {
            ThrowIfDisposed();

            await _task.WaitAsync(token);

            var tempFile = Interlocked.Exchange(ref _tempFile, null);
            if (tempFile is null) throw new InvalidOperationException();

            //Debug.WriteLine($"## Export: {_extractFileName}");
            return tempFile;
        }


        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    var tempFile = Interlocked.Exchange(ref _tempFile, null);
                    tempFile?.Dispose();
                }

                //Debug.WriteLine($"## Disposed: {_extractFileName}");
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
