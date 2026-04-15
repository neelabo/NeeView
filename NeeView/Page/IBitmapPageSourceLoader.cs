using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IBitmapPageSourceLoader
    {
        Task<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token);
    }
}
