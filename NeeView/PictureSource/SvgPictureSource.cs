using NeeView.Media.Imaging;
using NeeView.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class SvgPictureSource : IPictureSource<DrawingGroup>
    {
        public SvgPictureSource(ArchiveEntry archiveEntry, PictureInfo? pictureInfo)
        {
            ArchiveEntry = archiveEntry;
            PictureInfo = pictureInfo;
        }

        public ArchiveEntry ArchiveEntry { get; }

        public PictureInfo? PictureInfo { get; }


        public async ValueTask<byte[]> CreateImageAsync(DrawingGroup drawing, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            if (size.IsEmptyOrZero()) throw new ArgumentOutOfRangeException(nameof(size));

            token.ThrowIfCancellationRequested();

            var imageSource = CreateImageSource(drawing); // TODO: async
            await Task.CompletedTask;

            var bitmap = AppDispatcher.Invoke(() => imageSource.CreateThumbnail(size));

            using (var outStream = new MemoryStream())
            {
                var encoder = BitmapFactoryTools.CreateEncoder(format, quality);
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(outStream);
                return outStream.ToArray();
            }
        }

        public async ValueTask<ImageSource> CreateImageSourceAsync(DrawingGroup drawing, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return await Task.FromResult(CreateImageSource(drawing)); // TODO: async
        }

        public async ValueTask<byte[]> CreateThumbnailAsync(DrawingGroup drawing, ThumbnailProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Debug.Assert(PictureInfo != null);
            var size = PictureInfo.Size;
            
            size = ThumbnailProfile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting(true);
            return await CreateImageAsync(drawing, size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }

        private ImageSource CreateImageSource(DrawingGroup drawing)
        {
            var image = new DrawingImage();
            image.Drawing = drawing;
            image.Freeze();
            return image;
        }

        public Size FixedSize(Size size)
        {
            // SVGはサイズ制限なし
            Debug.Assert(PictureInfo != null);
            return this.PictureInfo.Size;
        }
    }
}
