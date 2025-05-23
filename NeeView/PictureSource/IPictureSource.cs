using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public interface IPictureSource
    {
        ArchiveEntry ArchiveEntry { get; }
        PictureInfo? PictureInfo { get; }

        Size FixedSize(Size size);
        ValueTask<ImageSource> CreateImageSourceAsync(object data, Size size, BitmapCreateSetting setting, CancellationToken token);
        ValueTask<byte[]> CreateImageAsync(object data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
        ValueTask<byte[]> CreateThumbnailAsync(object data, ThumbnailProfile profile, CancellationToken token);
    }


    public interface IPictureSource<T> : IPictureSource
    {
        async ValueTask<ImageSource> IPictureSource.CreateImageSourceAsync(object data, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            return await CreateImageSourceAsync((T)data, size, setting, token);
        }

        async ValueTask<byte[]> IPictureSource.CreateImageAsync(object data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            return await CreateImageAsync((T)data, size, setting, format, quality, token);
        }

        async ValueTask<byte[]> IPictureSource.CreateThumbnailAsync(object data, ThumbnailProfile profile, CancellationToken token)
        {
            return await CreateThumbnailAsync((T)data, profile, token);
        }

        ValueTask<ImageSource> CreateImageSourceAsync(T data, Size size, BitmapCreateSetting setting, CancellationToken token);
        ValueTask<byte[]> CreateImageAsync(T data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
        ValueTask<byte[]> CreateThumbnailAsync(T data, ThumbnailProfile profile, CancellationToken token);
    }

}
