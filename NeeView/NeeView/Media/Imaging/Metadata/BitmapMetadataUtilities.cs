using NeeView.Numetrics;
using System;
using System.Globalization;

namespace NeeView.Media.Imaging.Metadata
{
    public static class BitmapMetadataUtilities
    {
        public static object ExifUI8ToURational(object src)
        {
            if (src is ulong value)
            {
                return new URational((uint)(value & 0xffffffff), (uint)((value >> 32) & 0xffffffff));
            }

            return src;
        }

        public static object ExifI8ToRational(object src)
        {
            if (src is long value)
            {
                return new Rational((int)(value & 0xffffffff), (int)((value >> 32) & 0xffffffff));
            }

            return src;
        }

        public static object ExifStringToDateTime(object src)
        {
            if (src is string s)
            {
                var tokens = s.Split(' ');
                var newDateTime = tokens[0].Replace(':', '/') + " " + tokens[1];
                return DateTime.Parse(newDateTime, CultureInfo.InvariantCulture);
            }

            return src;
        }
    }
}
