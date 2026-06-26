using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace NeeView.Effects
{
    public static class ColorizeEffectTools
    {
        public static Brush CreateLutBrush(IList<ColorizeControlPoint> points)
        {
            if (points.Count == 0)
            {
                return Effect.ImplicitInput;
            }

            var n = points.Count;

            var knots = BuildKnots(points);

            var bmp = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgra32, null);

            var lut = new int[256];
            for (int x = 0; x < 256; x++)
            {
                var luminance = x / 255.0;

                int section = 0;
                while (section < n - 2 && !(knots[section] <= luminance && luminance <= knots[section + 1]))
                {
                    section++;
                }

                var v = (luminance - knots[section]) / (knots[section + 1] - knots[section]);
                v = Math.Clamp(v, 0.0, 1.0);

                var s0 = points[section + 0].Strength;
                var s1 = points[section + 1].Strength;

                var m = (s0 + s1 > 0) ? (s1 / (s0 + s1)) : 0.5;

                // Quadratic spline: Passing through (0,0), (0.5,m), and (1,1)
                var aa = 2.0 - 4.0 * m;
                var bb = 4.0 * m - 1.0;
                var v2 = aa * v * v + bb * v;
                v2 = Math.Clamp(v2, 0.0, 1.0);

                var c0 = points[section + 0].Color;
                var c1 = points[section + 1].Color;

                var r = (byte)(c0.R * (1.0f - v2) + c1.R * v2);
                var g = (byte)(c0.G * (1.0f - v2) + c1.G * v2);
                var b = (byte)(c0.B * (1.0f - v2) + c1.B * v2);
                var a = (byte)(c0.A * (1.0f - v2) + c1.A * v2);

                lut[x] = (a << 24) | (r << 16) | (g << 8) | b;
            }

            bmp.WritePixels(new Int32Rect(0, 0, 256, 1), lut, 256 * 4, 0);

            var brush = new ImageBrush(bmp);
            brush.Freeze();

            return brush;
        }

        private static double[] BuildKnots(IList<ColorizeControlPoint> p)
        {
            var n = p.Count;

            var length = new double[n - 1];
            double sum = 0.0;
            for (int i = 0; i < n - 1; i++)
            {
                length[i] = Math.Max((p[i].Strength + p[i + 1].Strength) * 0.5, 0.01);
                sum += length[i];
            }

            var knots = new double[n];
            knots[0] = 0.0;
            for (int i = 0; i < n - 1; i++)
            {
                knots[i + 1] = knots[i] + length[i] / sum;
            }

            return knots;
        }
    }


}