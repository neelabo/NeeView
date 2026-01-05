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
    }
}