using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public interface IPageThumbnailLoader
    {
        bool IsThumbnailValid { get; }

        ValueTask LoadThumbnailAsync(CancellationToken token);
    }

}
