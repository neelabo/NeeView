using NeeView.Windows.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView.Effects
{
    public static class BrushTools
    {
        public static Brush LuminanceBrush { get; } = CreateLuminanceSampleBrush();
        public static Brush ToneBrush { get; } = CreateToneSampeBrush();


        private static Brush CreateLuminanceSampleBrush()
        {
            var bmp = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgra32, null);

            var lut = new int[256];
            for (int x = 0; x < 256; x++)
            {
                lut[x] = (0xFF << 24) | (x << 16) | (x << 8) | x;
            }
            bmp.WritePixels(new Int32Rect(0, 0, 256, 1), lut, 256 * 4, 0);
            var brush = new ImageBrush(bmp);
            brush.Freeze();

            return brush;
        }

        private static Brush CreateToneSampeBrush()
        {
            var bmp = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgra32, null);

            var lut = new int[256];
            for (int x = 0; x < 256; x++)
            {
                var h = x / 255.0 * 360.0;
                var hsv = HSVColor.FromHSV(1.0, h, 1.0, 1.0);
                var rgb = hsv.ToARGB();
                lut[x] = (rgb.A << 24) | (rgb.R << 16) | (rgb.G << 8) | rgb.B;
            }
            bmp.WritePixels(new Int32Rect(0, 0, 256, 1), lut, 256 * 4, 0);
            var brush = new ImageBrush(bmp);
            brush.Freeze();

            return brush;
        }
    }

}
