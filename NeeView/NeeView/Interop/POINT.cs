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
