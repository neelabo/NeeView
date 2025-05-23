using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class ArchiveViewSourceStrategy : IViewSourceStrategy
    {
        public ArchiveViewSourceStrategy()
        {
        }

        public async ValueTask<DataSource> LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not ArchivePageData pageData) throw new InvalidOperationException(nameof(data.Data));

            var iconSource = await AppDispatcher.InvokeAsync(() =>
            {
                var bitmapSourceCollection = pageData.ArchiveEntry.IsDirectory
                    ? FileIconCollection.Current.CreateDefaultFolderIcon()
                    : FileIconCollection.Current.CreateFileIcon(pageData.ArchiveEntry.SystemPath, IO.FileIconType.FileType, true, true);
                bitmapSourceCollection.Freeze();
                return bitmapSourceCollection.GetBitmapSource(48.0);
            });

            ImageSource? imageSource = null;
            if (pageData.ThumbnailType == ThumbnailType.Unique)
            {
                var pageContent = pageData.PageContent;
                if (pageContent is not null && pageData.DataSource is not null)
                {
                    var dataSource = pageData.DataSource;
                    var strategy = ViewSourceStrategyFactory.Create(pageContent, dataSource);
                    if (strategy is ImageViewSourceStrategy imageViewSourceStrategy)
                    {
                        var viewSource = await imageViewSourceStrategy.LoadCoreAsync(dataSource, Size.Empty, token);
                        if (viewSource.Data is ImageViewData imageViewData)
                        {
                            imageSource = imageViewData.ImageSource;
                        }
                    }
                }
            }
            else
            {
                if (pageData.ThumbnailType != ThumbnailType.Empty)
                {
                    imageSource = Thumbnail.CreateImageSource(pageData.ThumbnailType);
                }
            }

            var dataSize = GetMemorySize(imageSource) + GetMemorySize(iconSource);
            return new DataSource(new ArchiveViewData(pageData.ArchiveEntry, imageSource, iconSource), dataSize, null);
        }

        private static long GetMemorySize(ImageSource? imageSource)
        {
            if (imageSource == null) return 0L;

            if (imageSource is BitmapSource bitmapSource)
            {
                return (long)bitmapSource.Format.BitsPerPixel * bitmapSource.PixelWidth * bitmapSource.PixelHeight / 8;
            }
            else
            {
                return 1024 * 1024;
            }
        }
    }

}
