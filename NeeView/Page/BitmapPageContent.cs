using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace NeeView
{
    public class BitmapPageContent : PageContent
    {
        private readonly BitmapPageContentLoader _loader;

        public BitmapPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
            _loader = new BitmapPageContentLoader();
        }

        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            var streamSource = new ArchiveEntryStreamSource(ArchiveEntry, Decrypt);
            var imageData = await _loader.LoadAsync(streamSource, true, false, token);
            return imageData.PictureInfo;
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                token.ThrowIfCancellationRequested();

                // NOTE: 開発テスト用の読み込み処理遅延エミュレート
#if false
#if DEBUG
                if (Debugger.IsAttached)
                {
                    //Debug.WriteLine($"Loading...: {ArchiveEntry}");
                    await Task.Delay(200, token);
                    NVDebug.AssertMTA();
                }
#endif
#endif

                var streamSource = new ArchiveEntryStreamSource(ArchiveEntry, Decrypt);
                await streamSource.CreateCacheAsync(Decrypt, token);

                var createPictureInfo = PictureInfo is null;
                var imageData = await _loader.LoadAsync(streamSource, createPictureInfo, true, token);
                return imageData;
            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }
    }
}
