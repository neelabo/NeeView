using System;

namespace NeeLaboratory.Text
{
    public static class SpanExtensions
    {
        public static string ToNullTerminatedString(this Span<char> span)
        {
            int len = span.IndexOf('\0');
            if (len < 0)
            {
                return span.ToString();
            }

            return span[..len].ToString();
        }

        public static string ToNullTerminatedString(this ReadOnlySpan<char> span)
        {
            int len = span.IndexOf('\0');
            if (len < 0)
            {
                return span.ToString();
            }

            return span[..len].ToString();
        }
    }

}
