using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public static class BitmapSourceExtension
    {
        private static readonly string _defaultFormatLabel = PixelFormats.Default.ToString();

        // from http://www.nminoru.jp/~nminoru/programming/bitcount.html
        public static int BitCount(int bits)
        {
            bits = (bits & 0x55555555) + (bits >> 1 & 0x55555555);
            bits = (bits & 0x33333333) + (bits >> 2 & 0x33333333);
            bits = (bits & 0x0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f);
            bits = (bits & 0x00ff00ff) + (bits >> 8 & 0x00ff00ff);
            return (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
        }

        // from http://www.nminoru.jp/~nminoru/programming/bitcount.html
        public static int BitNTZ(int bits)
        {
            return BitCount((~bits) & (bits - 1));
        }

        // GetOneColor()のサポートフォーマット
        private static readonly PixelFormat[] _supportedFormats = new PixelFormat[]
        {
            PixelFormats.Bgra32,
            PixelFormats.Bgr32,
            PixelFormats.Bgr24,
            PixelFormats.Bgr555,
            PixelFormats.Bgr565,
            PixelFormats.Gray8,
            PixelFormats.Gray4,
            PixelFormats.Gray2,
        };

        // GetOneColor()のサポートフォーマット (インデックスカラー)
        private static readonly PixelFormat[] _supportedIndexFormats = new PixelFormat[]
        {
            PixelFormats.Indexed8,
            PixelFormats.Indexed4,
            PixelFormats.Indexed2,
            PixelFormats.Indexed1,
        };

        private static readonly PixelFormat[] _supportedAlphaFormats = new PixelFormat[]
        {
            PixelFormats.Prgba64,
            PixelFormats.Prgba128Float,
            PixelFormats.Pbgra32,
            PixelFormats.Bgra32,
            PixelFormats.Rgba128Float,
            PixelFormats.Rgba64,
        };


        /// <summary>
        /// 有効BitsPerPixelを取得する
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static int GetSourceBitsPerPixel(this BitmapSource bitmap)
        {
            if (bitmap == null) return 0;
            return bitmap.Format.BitsPerPixel;
        }


        /// <summary>
        /// 画像の最初の1ピクセルのカラーを取得
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Color GetOneColor(this BitmapSource bitmap)
        {
            if (bitmap == null) return Colors.Black;

            // 1pixel取得
            var pixels = new int[1];
            bitmap.CopyPixels(new System.Windows.Int32Rect(0, 0, 1, 1), pixels, 4, 0);

            // ビットマスクを適用して要素の値を取得する
            var elements = new List<byte>();
            foreach (PixelFormatChannelMask channelMask in bitmap.Format.Masks)
            {
                int bits = 0;
                int index = 0;

                foreach (byte myByte in channelMask.Mask)
                {
                    bits |= (myByte << (index++ * 8));
                }

                int shift = BitNTZ(bits);

                elements.Add((byte)((pixels[0] & bits) >> shift));
            }

            var color = new Color();

            if (_supportedFormats.Contains(bitmap.Format))
            {
                color.B = elements[0];
                color.G = (elements.Count >= 2) ? elements[1] : elements[0];
                color.R = (elements.Count >= 3) ? elements[2] : elements[0];
                color.A = 0xFF; // elements[3];
            }
            else if (_supportedIndexFormats.Contains(bitmap.Format))
            {
                color = bitmap.Palette.Colors[elements[0]];
                color.A = 0xFF;
            }
            else
            {
                Debug.WriteLine("GetOneColor: No support format: " + bitmap.Format.ToString());
                color = Colors.Black;
            }

            return color;
        }

        /// <summary>
        /// 半透明画像であるか判定
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static bool? HasAlpha(this BitmapSource bitmap)
        {
            // DelayCreation などでフォーマットが確定していない状態。
            if (bitmap.Format.ToString() == _defaultFormatLabel)
            {
                return null;
            }

            return _supportedAlphaFormats.Contains(bitmap.Format) || bitmap.Palette?.Colors.Any(e => e.A < 0xff) == true;
        }
    }
}
