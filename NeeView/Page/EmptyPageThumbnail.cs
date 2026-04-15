using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class EmptyPageThumbnail : PageThumbnail
    {
        private readonly EmptyPageContent _content;

        public EmptyPageThumbnail(EmptyPageContent content) : base(content)
        {
            _content = content;
        }

        public override async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            return new ThumbnailSource(ThumbnailType.NoEntry);
        }
    }

}
