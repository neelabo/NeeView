using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static POINT operator +(POINT pt, POINT sz) => Add(pt, sz);

        public static POINT operator -(POINT pt, POINT sz) => Subtract(pt, sz);

        public static POINT Add(POINT pt, POINT sz) => new POINT(pt.x + sz.x, pt.y + sz.y);

        public static POINT Subtract(POINT pt, POINT sz) => new POINT(pt.x - sz.x, pt.y - sz.y);

        public override string ToString()
        {
            return $"(x={x}, y={y})";
        }

        public static POINT Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new InvalidCastException();

            var tokens = s.Split(',');
            if (tokens.Length != 2) throw new InvalidCastException();

            return new POINT(int.Parse(tokens[0], CultureInfo.InvariantCulture), int.Parse(tokens[1], CultureInfo.InvariantCulture));
        }
    }
}
