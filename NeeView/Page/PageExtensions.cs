using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace NeeView
{
    public static class PageExtensions
    {
        /// <summary>
        /// Create Page visual for Dialog thumbnail.
        /// </summary>
        /// <returns>Image</returns>
        public static async ValueTask<Image> CreatePageVisualAsync(this Page page)
        {
            // NOTE: 確実に非同期で処理させるため
            var imageSource = await Task.Run(async () =>
            {
                await page.LoadThumbnailAsync(CancellationToken.None);
                return page.Thumbnail.CreateImageSource();
            });

            var image = new Image();
            image.Source = imageSource;
            image.Effect = new DropShadowEffect()
            {
                Opacity = 0.5,
                ShadowDepth = 2,
                RenderingBias = RenderingBias.Quality
            };
            image.MaxWidth = 96;
            image.MaxHeight = 96;

            return image;
        }
    }

}
