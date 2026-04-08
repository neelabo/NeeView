using NeeLaboratory.Text;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;


namespace NeeView.StringTemplate
{
    public static partial class StringFormatTools
    {
        public static string FormatValue(string format, object value)
        {
            if (value is string s && format != "")
            {
                return StringFormat(format, s);
            }
            else
            {
                var fmt = format == "" ? "{0}" : "{0:" + format + "}";
                return string.Format(CultureInfo.InvariantCulture, fmt, value);
            }
        }


        public static string StringFormat(string format, string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }

            if (format[0] == '/')
            {
                return ReplaceSeparator(format[1..], s);
            }
            else
            {
                return StringFormatValue(format, s);
            }
        }

        public static string ReplaceSeparator(string format, string value)
        {
            var separator = string.Concat(format.EscapeStringWalker().Select(e => e.Char));
            var parts = value.Split(LoosePath.Separators, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(separator, parts);
        }

        public static string StringFormatValue(string format, string value)
        {
            var numberSign = new EscapedChar('#');
            var count = format.EscapeStringWalker().Count(e => e == numberSign);

            int index = 0;
            var sb = new StringBuilder();

            foreach (var c in format.EscapeStringWalker())
            {
                if (c == numberSign)
                {
                    var x = GetStringFromEnd(value, index, count);
                    sb.Append(x);
                    index++;
                }
                else
                {
                    sb.Append(c.Char);
                }
            }

            return sb.ToString();
        }

        private static string GetStringFromEnd(string s, int index, int size)
        {
            Debug.Assert(index < size);
            if (size <= index)
            {
                return "";
            }

            var p = s.Length - (size - index);
            if (p < 0)
            {
                return "";
            }
            else if (index == 0)
            {
                return s[0..(p + 1)];
            }
            else
            {
                return s[p].ToString();
            }
        }

    }

}
