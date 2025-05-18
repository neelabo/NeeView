using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class EmptyPageThumbnail : PageThumbnail
    {
        private readonly EmptyPageContent _content;

        public EmptyPageThumbnail(EmptyPageContent content) : base(content)
        {
            _content = content;
        }

        public override async ValueTask<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            return await Task.FromResult(new ThumbnailSource(ThumbnailType.NoEntry));
        }
    }

}
