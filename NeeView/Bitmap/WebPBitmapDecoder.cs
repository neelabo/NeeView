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
            if (size != Size.Empty)
            {
                // サイズ変更する場合は MagicScaler にまかせる。 MagicScaler の WebP 処理は半透明問題は発生しない。
                var decoder = new MagicScalerBitmapDecoder();
                return decoder.Create(stream, info, size, token);
            }

            // TODO: これだけlibwebpでなく.NETライブラリに依存しているのでよろしくない。
            info = info ?? BitmapInfo.Create(stream);

            // Decode
            var bytes = stream.ReadAllBytes();
            var webp = WebPDecoder.Decode(bytes);
            var bitmap = webp.ToBitmapSource();

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
            if (size != Size.Empty)
            {
                // サイズ変更する場合は MagicScaler にまかせる。 MagicScaler の WebP 処理は半透明問題は発生しない。
                var decoder = new MagicScalerBitmapDecoder();
                decoder.CreateImage(stream, info, outStream, size, format, quality, token);
                return;
            }

            BitmapSource bitmap = Create(stream, info, size, token);
            BitmapFactoryTools.OutputImage(bitmap, outStream, format, quality);
        }
    }
}