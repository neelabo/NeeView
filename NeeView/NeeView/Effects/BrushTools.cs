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
        public static Brush Check4x4Brush { get; } = CreateChecker4x4Brush();

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

        public static Brush CreateChecker4x4Brush()
        {
            // from http://msdn.microsoft.com/en-us/library/aa970904.aspx

            var checkerBrush = new DrawingBrush();

            var backgroundSquare =
                new GeometryDrawing(
                    Brushes.White,
                    null,
                    new RectangleGeometry(new Rect(0, 0, 8, 8)));

            var aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 4, 4)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(4, 4, 4, 4)));

            var checkers = new GeometryDrawing(Brushes.Black, null, aGeometryGroup);

            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            checkerBrush.Drawing = checkersDrawingGroup;
            checkerBrush.Viewport = new Rect(0, 0, 0.5, 0.5);
            checkerBrush.TileMode = TileMode.Tile;
            checkerBrush.Freeze();

            return checkerBrush;
        }
    }

}
