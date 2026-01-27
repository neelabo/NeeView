using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IPageThumbnailLoader
    {
        bool IsThumbnailValid { get; }

        ValueTask LoadThumbnailAsync(CancellationToken token);
    }

}
