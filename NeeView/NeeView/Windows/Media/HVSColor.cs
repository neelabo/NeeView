// http://sourcechord.hatenablog.com/entry/20130710/1373476676

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media;


namespace NeeView.Windows.Media
{
    /// <summary>
    /// HSVカラー
    /// </summary>
    public struct HSVColor
    {
        #region Fields, Properties
        public double A { get; set; }
        public double H { get; set; }
        public double S { get; set; }
        public double V { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// HSV生成
        /// </summary>
        /// <param name="a"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static HSVColor FromHSV(double a, double h, double s, double v)
        {
            return new HSVColor() { A = a, H = h, S = s, V = v };
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "A:{0}, H:{1}, S:{2}, V:{3}", A, H, S, V);
        }

        /// <summary>
        /// ARGBカラーで出力
        /// </summary>
        /// <returns></returns>
        public Color ToARGB()
        {
            int Hi = ((int)(H / 60.0)) % 6;
            double f = H / 60.0f - (int)(H / 60.0);
            double p = V * (1 - S);
            double q = V * (1 - f * S);
            double t = V * (1 - (1 - f) * S);

            return Hi switch
            {
                0 => FromARGB(A, V, t, p),
                1 => FromARGB(A, q, V, p),
                2 => FromARGB(A, p, V, t),
                3 => FromARGB(A, p, q, V),
                4 => FromARGB(A, t, p, V),
                5 => FromARGB(A, V, p, q),
                _ => throw new InvalidOperationException(),
            };
        }

        /// <summary>
        /// fRGBからARGBに変換
        /// </summary>
        /// <param name="fr"></param>
        /// <param name="fg"></param>
        /// <param name="fb"></param>
        /// <returns></returns>
        public static Color FromRGB(double fr, double fg, double fb)
        {
            fr *= 255;
            fg *= 255;
            fb *= 255;
            byte r = (byte)((fr < 0) ? 0 : (fr > 255) ? 255 : fr);
            byte g = (byte)((fg < 0) ? 0 : (fg > 255) ? 255 : fg);
            byte b = (byte)((fb < 0) ? 0 : (fb > 255) ? 255 : fb);
            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// fARGBからARGBに変換
        /// </summary>
        /// <param name="fa"></param>
        /// <param name="fr"></param>
        /// <param name="fg"></param>
        /// <param name="fb"></param>
        /// <returns></returns>
        private static Color FromARGB(double fa, double fr, double fg, double fb)
        {
            fa *= 255;
            fr *= 255;
            fg *= 255;
            fb *= 255;
            byte a = (byte)((fa < 0) ? 0 : (fa > 255) ? 255 : fa);
            byte r = (byte)((fr < 0) ? 0 : (fr > 255) ? 255 : fr);
            byte g = (byte)((fg < 0) ? 0 : (fg > 255) ? 255 : fg);
            byte b = (byte)((fb < 0) ? 0 : (fb > 255) ? 255 : fb);
            return Color.FromArgb(a, r, g, b);
        }

        #endregion
    }


    /// <summary>
    /// HSVカラー拡張
    /// </summary>
    public static class HSVColorExtension
    {
        /// <summary>
        /// ARGB を HSV に変換
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static HSVColor ToHSV(this Color c)
        {
            double a = c.A / 255.0;
            double r = c.R / 255.0;
            double g = c.G / 255.0;
            double b = c.B / 255.0;

            var list = new double[] { r, g, b };
            var max = list.Max();
            var min = list.Min();

            double h, s, v;
            if (max == min)
                h = 0;
            else if (max == r)
                h = (60 * (g - b) / (max - min) + 360) % 360;
            else if (max == g)
                h = 60 * (b - r) / (max - min) + 120;
            else
                h = 60 * (r - g) / (max - min) + 240;

            if (max == 0)
                s = 0;
            else
                s = (max - min) / max;

            v = max;

            return new HSVColor() { A = a, H = h, S = s, V = v };
        }
    }
}
