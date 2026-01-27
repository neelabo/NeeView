using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PdfPageContent : PageContent
    {
        private readonly PdfArchive _pdfArchive;

        public PdfPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
            _pdfArchive = archiveEntry.Archive as PdfArchive ?? throw new InvalidOperationException();
        }

        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            var pictureInfo = CreatePictureInfo(token);
            return await Task.FromResult(pictureInfo);
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            await Task.CompletedTask;

            try
            {
                token.ThrowIfCancellationRequested();
                var pictureInfo = CreatePictureInfo(token);
                return PageSource.Create(new PdfPageData(ArchiveEntry), pictureInfo);
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

        private PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (PictureInfo != null) return PictureInfo;

            var pictureInfo = new PictureInfo();
            var originalSize = _pdfArchive.GetSourceSize(ArchiveEntry); // TODO: async
            pictureInfo.OriginalSize = originalSize;
            var maxSize = Config.Current.Performance.MaximumSize;
            var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : originalSize;
            pictureInfo.Size = size;
            pictureInfo.BitsPerPixel = 32;
            pictureInfo.Decoder = _pdfArchive.ToString();
            return pictureInfo;
        }
    }
}
