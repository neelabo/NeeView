using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IPageThumbnailLoader
    {
        bool IsThumbnailValid { get; }

        Task LoadThumbnailAsync(CancellationToken token);
    }

}
