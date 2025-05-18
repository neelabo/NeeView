using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class BitmapPageContentLoader
    {
        private IBitmapPageSourceLoader? _imageDataLoader;

        public BitmapPageContentLoader()
        {
        }

        public async ValueTask<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token)
        {
            var loader = _imageDataLoader ?? new BitmapPageSourceLoader();
            var imageData = await loader.LoadAsync(streamSource, createPictureInfo, createSource, token);
            _imageDataLoader = imageData.ImageDataLoader;
            return imageData;
        }
    }
}
