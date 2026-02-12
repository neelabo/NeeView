using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class WebPBitmapDecoder : IBitmapDecoder
    {
        public string Name => "libwebp";


        public bool CheckFormat(Stream stream)
        {
            return AnimatedImageChecker.IsWebp(stream);
        }

        public BitmapSource Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token)
        {
            info = info ?? BitmapInfo.Create(stream);

            // Decode
            stream.Seek(0, SeekOrigin.Begin);
            BitmapSource bitmap = WebPDecoder.Decode(stream);
            if (!size.IsEmptyOrZero() || info.IsTranspose)
            {
                bitmap = ScaleTransformBitmapSource(bitmap, size, info.IsTranspose);
            }
            bitmap.Freeze();

            // Transform
            if (info.IsMirrorHorizontal || info.IsMirrorVertical || info.Rotation != Rotation.Rotate0)
            {
                var transform = new TransformGroup();
                if (info.IsMirrorHorizontal || info.IsMirrorVertical)
                {
                    transform.Children.Add(new ScaleTransform
                    {
                        ScaleX = info.IsMirrorHorizontal ? -1 : 1,
                        ScaleY = info.IsMirrorVertical ? -1 : 1,
                    });
                }
                if (info.Rotation != Rotation.Rotate0)
                {
                    transform.Children.Add(new RotateTransform(info.Rotation.ToAngle()));
                }

                var transformBitmap = new TransformedBitmap();
                transformBitmap.BeginInit();
                transformBitmap.Source = bitmap;
                transformBitmap.Transform = transform;
                transformBitmap.EndInit();
                transformBitmap.Freeze();
                return transformBitmap;
            }
            else
            {
                return bitmap;
            }
        }

        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token)
        {
            BitmapSource bitmap = Create(stream, info, size, token);
            BitmapFactoryTools.OutputImage(bitmap, outStream, format, quality);
        }


        private static BitmapSource ScaleTransformBitmapSource(BitmapSource source, Size size, bool isTranspose)
        {
            if (size.IsEmptyOrZero())
            {
                return source;
            }

            int pixelWidth = isTranspose ? (int)size.Height : (int)size.Width;
            int pixelHeight = isTranspose ? (int)size.Width : (int)size.Height;
            var scaleX = (double)pixelWidth / source.PixelWidth;
            var scaleY = (double)pixelHeight / source.PixelHeight;
            if (pixelWidth == 0) scaleX = scaleY;
            if (pixelHeight == 0) scaleY = scaleX;

            var scale = new ScaleTransform(scaleX, scaleY, 0, 0);
            var resized = new TransformedBitmap(source, scale);
            var finalWb = new WriteableBitmap(resized);

            return finalWb;
        }

    }
}