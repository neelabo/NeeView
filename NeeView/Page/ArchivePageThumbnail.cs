using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ArchivePageThumbnail : PageThumbnail
    {
        private readonly ArchivePageContent _content;

        public ArchivePageThumbnail(ArchivePageContent content) : base(content)
        {
            _content = content;
            this.Thumbnail.IsCacheEnabled = true;
        }

        public override async ValueTask<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NVDebug.AssertMTA();

            var pageContent = await ArchivePageUtility.GetSelectedPageContentAsync(_content.ArchiveEntry, false, token);
            pageContent.Decrypt = false;
            if (pageContent is ArchivePageContent)
            {
                if (pageContent.ArchiveEntry.IsMedia())
                {
                    return new ThumbnailSource(ThumbnailType.Media);
                }
                else
                {
                    return new ThumbnailSource(ThumbnailType.Empty);
                }
            }
            else
            {
                Debug.Assert(pageContent is not ArchivePageContent);
                var pageThumbnail = PageThumbnailFactory.Create(pageContent);
                return await pageThumbnail.LoadThumbnailAsync(token);
            }
        }
    }

}
