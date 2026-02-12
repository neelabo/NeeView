using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 32bit BGRA Raw Image
    /// </summary>
    public sealed class RawImage
    {
        // TODO: 複数のピクセルフォーマットに対応する。
        public RawImage(byte[] buffer, int width, int height)
        {
            Buffer = buffer;
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Buffer { get; }
        public int Stride => Width * 4;
    }


    public static class RawImageExtensions
    {
        public static BitmapSource ToBitmapSource(this RawImage image)
        {
            var bitmap = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, image.Buffer, image.Stride);
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapSource ToBitmapSource(this RawImage image, Size size, bool isTranspose)
        {
            if (size.IsEmptyOrZero())
            {
                return image.ToBitmapSource();
            }

            int pixelWidth = isTranspose ? (int)size.Height : (int)size.Width;
            int pixelHeight = isTranspose ? (int)size.Width : (int)size.Height;

            var scaleX = (double)pixelWidth / image.Width;
            var scaleY = (double)pixelHeight / image.Height;
            if (pixelWidth == 0) scaleX = scaleY;
            if (pixelHeight == 0) scaleY = scaleX;

            var wb = image.CreateBitmapFromBGRA();
            var scale = new ScaleTransform(scaleX, scaleY, 0, 0);
            var resized = new TransformedBitmap(wb, scale);
            var finalWb = new WriteableBitmap(resized);
            finalWb.Freeze();

            return finalWb;
        }

        /// <summary>
        /// RawImage から WritableBitmap への高速変換 (unsafe)
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static unsafe WriteableBitmap CreateBitmapFromBGRA(this RawImage image)
        {
            var wb = new WriteableBitmap(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null);

            wb.Lock();

            try
            {
                byte* dst = (byte*)wb.BackBuffer.ToPointer();
                fixed (byte* src = image.Buffer)
                {
                    Buffer.MemoryCopy(src, dst, image.Stride * image.Height, image.Stride * image.Height);
                }

                wb.AddDirtyRect(new Int32Rect(0, 0, image.Width, image.Height));
            }
            finally
            {
                wb.Unlock();
            }

            return wb;
        }

    }
}