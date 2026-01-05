using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public static class BitmapFactoryTools
    {
        public static void OutputImage(BitmapSource bitmap, Stream outStream, BitmapImageFormat format, int quality)
        {
            if (bitmap.DpiX != bitmap.DpiY)
            {
                Debug.WriteLine($"WrongDPI: {bitmap.DpiX},{bitmap.DpiY}");

                double dpi = 96;
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                int stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
                stride = (stride + 3) / 4 * 4; // 不要かも？

                byte[] pixelData = new byte[stride * height];
                bitmap.CopyPixels(pixelData, stride, 0);

                bitmap = BitmapSource.Create(width, height, dpi, dpi, bitmap.Format, bitmap.Palette, pixelData, stride);
            }

            var encoder = CreateEncoder(format, quality);
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(outStream);
        }

        public static BitmapEncoder CreateEncoder(BitmapImageFormat format, int quality)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    var jpegEncoder = new JpegBitmapEncoder();
                    jpegEncoder.QualityLevel = quality;
                    return jpegEncoder;
                case BitmapImageFormat.Png:
                    return new PngBitmapEncoder();
            }
        }
    }
}