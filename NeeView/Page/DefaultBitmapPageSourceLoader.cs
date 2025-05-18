using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DefaultBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async ValueTask<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token)
        {
            try
            {
                var pictureInfo = createPictureInfo ? await PictureInfo.CreateAsync(streamSource, ".NET BitmapImage", token) : null;
                await Task.CompletedTask;
                var data = createSource ? new BitmapPageData(streamSource) : null;
                return BitmapPageSource.Create(data, pictureInfo, this);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return BitmapPageSource.CreateError(ex.Message);
            }
        }
    }
}
