using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IBitmapPageSourceLoader
    {
        ValueTask<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token);
    }
}
