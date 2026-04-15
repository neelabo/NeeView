using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class MediaPageThumbnail : PageThumbnail
    {
        private readonly MediaPageContent _content;

        public MediaPageThumbnail(MediaPageContent content) : base(content)
        {
            _content = content;
        }

        public override async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            return new ThumbnailSource(ThumbnailType.Media);
        }
    }

}
