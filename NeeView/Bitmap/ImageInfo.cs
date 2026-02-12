using System.Windows.Media;

namespace NeeView
{
    public class ImageInfo
    {
        public int PixelWidth { get; init; }
        public int PixelHeight { get; init; }
        public PixelFormat Format { get; init; } = PixelFormats.Bgra32;
        public int BitsPerPixel { get; init; } = 32;
        public int FrameCount { get; init; } = 1;
        public double DpiX { get; init; } = 96.0;
        public double DpiY { get; init; } = 96.0;
        public double Width => PixelWidth * 96.0 / DpiX;
        public double Height => PixelHeight * 96.0 / DpiY;
        public bool HasAlpha { get; init; }
    }
}
