using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedViewSourceStrategy : IViewSourceStrategy
    {

        public AnimatedViewSourceStrategy()
        {
        }


        public async ValueTask<DataSource> LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not AnimatedPageData pageData) throw new InvalidOperationException(nameof(data.Data));

            // TODO: この画像が何度も読み込まれてないか調査すること
            var image = await LoadImageAsync(pageData.MediaSource, token);
            await Task.CompletedTask;

            // 色情報とBPP設定。
            if (image is not null)
            {
                data.PictureInfo?.SetPixelInfo(image);
            }

            var viewData = new AnimatedViewData(pageData.MediaSource, image);
            return new DataSource(viewData, 0, null);
        }


        // TODO: Async
        private async ValueTask<BitmapImage?> LoadImageAsync(MediaSource mediaSource, CancellationToken token)
        {
            try
            {
                using (var stream = await mediaSource.OpenStreamAsync(token))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoadFailed: {ex.Message}");
                return null;
            }
        }
    }
}
