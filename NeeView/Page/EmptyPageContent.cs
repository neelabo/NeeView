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

        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            return await Task.FromResult(new PictureInfo(DefaultSize));
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            return await Task.FromResult(new PageSource(new EmptyPageData(), null, new PictureInfo(DefaultSize)));
        }
    }

    public class EmptyPageData
    {
    }
}
