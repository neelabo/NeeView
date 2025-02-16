using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public int Width
        {
            get => right - left;
            set => right = left + value;
        }

        public int Height
        {
            get => bottom - top;
            set => bottom = top + value;
        }

        public override string ToString()
        {
            return $"(x={left}, y={top}, width={right - left}, height={bottom - top})";
        }

        public static RECT Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new InvalidCastException();

            var tokens = s.Split(',');
            if (tokens.Length != 4) throw new InvalidCastException();

            return new RECT(int.Parse(tokens[0], CultureInfo.InvariantCulture), int.Parse(tokens[1], CultureInfo.InvariantCulture), int.Parse(tokens[2], CultureInfo.InvariantCulture), int.Parse(tokens[3], CultureInfo.InvariantCulture));
        }
    }
}
