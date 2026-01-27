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

        public override async ValueTask<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            return await Task.FromResult(new ThumbnailSource(ThumbnailType.Media));
        }
    }

}
