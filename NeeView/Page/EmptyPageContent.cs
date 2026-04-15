using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class EmptyPageContent : PageContent
    {
        public EmptyPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
        }

        public override PageType PageType => PageType.Empty;

        protected override Task<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            return Task.FromResult<PictureInfo?>(new PictureInfo(DefaultSize));
        }

        protected override Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            return Task.FromResult(new PageSource(new EmptyPageData(), null, new PictureInfo(DefaultSize)));
        }
    }

    public class EmptyPageData
    {
    }
}
