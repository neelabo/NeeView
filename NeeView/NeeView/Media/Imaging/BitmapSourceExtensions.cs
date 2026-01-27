using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging
{
    public static class BitmapSourceExtensions
    {
        /// <summary>
        /// ビューコントロールをレンダリングしてBitmapにする
        /// </summary>
        /// <param name="visual">Width,Heightが設定されたコントロール</param>
        /// <returns>レンダリングされた画像</returns>
        public static BitmapSource CreateRenderBitmap(this FrameworkElement visual)
        {
            visual.Measure(new Size(visual.Width, visual.Height));
            visual.Arrange(new Rect(new Size(visual.Width, visual.Height)));
            visual.UpdateLayout();

            double dpi = 96.0;
            var bmp = new RenderTargetBitmap((int)visual.Width, (int)visual.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(visual);
            bmp.Freeze();

            return bmp;
        }

        /// <summary>
        /// サムネイル作成  (STA)
        /// </summary>
        public static BitmapSource? CreateThumbnail(this ImageSource source, Size maxSize)
        {
            if (source == null) return null;

            double width = source.GetPixelWidth();
            double height = source.GetPixelHeight();

            // maxSize.Height が nan のときはバナー
            bool isBanner = double.IsNaN(maxSize.Height);

            var scaleX = width > maxSize.Width ? maxSize.Width / width : 1.0;
            var scaleY = height > maxSize.Height ? maxSize.Height / height : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;
            if (scale > 1.0) scale = 1.0;

            if (scale < 0.99)
            {
                width = (int)(width * scale + 0.5) / 2 * 2;
                height = (int)(height * scale + 0.5) / 2 * 2;
                if (width < 2.0) width = 2.0;
                if (height < 2.0) height = 2.0;
            }

            if (Application.Current == null) return null;

            var canvas = new Canvas();
            canvas.Width = width;
            canvas.Height = height;

            var image = new Image();
            image.Source = source;
            image.Width = width;
            image.Height = height;
            image.Stretch = Stretch.Fill;

            double bannerHeight = (int)(width * 0.25);
            if (isBanner && bannerHeight < height)
            {
                canvas.Height = bannerHeight;

                double top = -(int)(height * 0.3 - bannerHeight * 0.5);
                if (top < -height) top = -height;
                if (top > 0) top = 0;
                Canvas.SetTop(image, top);
            }

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            image.UseLayoutRounding = true;

            canvas.Children.Add(image);

            // ビューツリー外でも正常にレンダリングするようにする処理
            canvas.Measure(new Size(canvas.Width, canvas.Height));
            canvas.Arrange(new Rect(new Size(canvas.Width, canvas.Height)));
            canvas.UpdateLayout();

            double dpi = 96.0;
            var bmp = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(canvas);
            bmp.Freeze();

            return bmp;
        }



        /// <summary>
        /// サムネイル作成(DrawingVisual版)
        /// 完全非同期にできるが、品質が悪い
        /// </summary>
        /// <param name="source"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static BitmapSource? CreateThumbnailByDrawingVisual(this ImageSource source, Size maxSize)
        {
            if (source == null) return null;

            double width = source.GetPixelWidth();
            double height = source.GetPixelHeight();

            var scaleX = width > maxSize.Width ? maxSize.Width / width : 1.0;
            var scaleY = height > maxSize.Height ? maxSize.Height / height : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;
            if (scale > 1.0) scale = 1.0;

            if (scale < 0.99)
            {
                width = (int)(width * scale + 0.5) / 2 * 2;
                height = (int)(height * scale + 0.5) / 2 * 2;
                if (width < 2.0) width = 2.0;
                if (height < 2.0) height = 2.0;
            }

            var visual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);

            using (var context = visual.RenderOpen())
            {
                context.DrawImage(source, new Rect(0, 0, width, height));
            }

            double dpi = 96.0;
            var bmp = new RenderTargetBitmap((int)width, (int)height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(visual);
            bmp.Freeze();

            return bmp;
        }
    }

    public static class ImageSourceExtensions
    {
        public static int GetPixelWidth(this ImageSource imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                return bitmapSource.PixelWidth;
            }
            else
            {
                return (int)imageSource.Width;
            }
        }

        public static int GetPixelHeight(this ImageSource imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                return bitmapSource.PixelHeight;
            }
            else
            {
                return (int)imageSource.Height;
            }
        }
    }
}
