using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;

namespace NeeView
{
    public interface IPageThumbnailLoader
    {
        bool IsThumbnailValid { get; }

        ValueTask<ImageSource?> LoadThumbnailAsync(CancellationToken token);
    }

}
