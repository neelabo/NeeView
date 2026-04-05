using System;
using System.Collections.Generic;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// String Format Parser
    /// </summary>
    public static class StringFormatParser
    {
        /// <summary>
        /// 簡易的な String Format パーサー
        /// </summary>
        /// <param name="format">string format</param>
        /// <returns>
        /// Words ... 変数部分のリスト。{Book} → "Book", {Page1:000} → "Page1:000" など。
        /// NewFormat ... 変数部分を{0},{1}.. に置き換えたもの。
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        public static (List<string> Words, string NewFormat) Parse(string format)
        {
            if (format is null) throw new ArgumentNullException(nameof(format));

            var words = new List<string>();
            var sb = new StringBuilder();

            int i = 0;
            while (i < format.Length)
            {
                char c = format[i];

                if (c == '{' && i + 1 < format.Length && format[i + 1] == '{')
                {
                    sb.Append("{{");
                    i += 2;
                    continue;
                }

                if (c == '}' && i + 1 < format.Length && format[i + 1] == '}')
                {
                    sb.Append("}}");
                    i += 2;
                    continue;
                }

                if (c == '{')
                {
                    int start = i;
                    int end = format.IndexOf('}', start + 1);
                    if (end < 0)
                    {
                        throw new FormatException("Missing '}' for placeholder.");
                    }

                    string inside = format.Substring(start + 1, end - start - 1);

                    if (inside.Contains("{") || inside.Contains("}"))
                    {
                        throw new FormatException($"Invalid placeholder '{{{inside}}}'.");
                    }

                    int index = words.Count;
                    words.Add(inside);

                    sb.Append('{').Append(index).Append('}');
                    i = end + 1;
                    continue;
                }

                if (c == '}')
                {
                    throw new FormatException($"Unexpected '}}' at position {i}.");
                }

                sb.Append(c);
                i++;
            }

            return (words, sb.ToString());
        }
    }
}

