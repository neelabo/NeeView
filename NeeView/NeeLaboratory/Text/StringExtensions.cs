using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeLaboratory.Text
{
    public static partial class StringExtensions
    {
        [GeneratedRegex(@"[\r\n]+")]
        private static partial Regex _newLineRegex { get; }

        private enum UnescapeState
        {
            Unescaped,
            Escaped
        }

        // from https://stackoverflow.com/questions/40347168/how-to-parse-an-escape-sequence
        public static string Unescape(this string s)
        {
            var sb = new StringBuilder(s.Length + 2);
            var state = UnescapeState.Unescaped;

            foreach (var ch in s)
            {
                switch (state)
                {
                    case UnescapeState.Escaped:
                        switch (ch)
                        {
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;

                            case '\\':
                            case '\"':
                                sb.Append(ch);
                                break;

                            default:
                                //throw new Exception("Unrecognized escape sequence '\\" + ch + "'");
                                sb.Append('\\');
                                sb.Append(ch);
                                break;
                        }
                        state = UnescapeState.Unescaped;
                        break;

                    case UnescapeState.Unescaped:
                        if (ch == '\\')
                        {
                            state = UnescapeState.Escaped;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            if (state == UnescapeState.Escaped)
            {
                //throw new Exception("Unterminated escape sequence");
                sb.Append('\\');
            }

            return sb.ToString();
        }

        /// <summary>
        /// 先頭を大文字にした TitleCase 文字列を作成する
        /// </summary>
        /// <param name="s">入力文字列</param>
        /// <param name="complete">先頭以外の文字を小文字にする</param>
        /// <returns></returns>
        public static string ToTitleCase(this string s, bool complete = false)
        {
            var a = s.Take(1).Select(e => char.ToUpper(e, CultureInfo.InvariantCulture));
            var b = complete ? s.Skip(1).Select(e => char.ToLower(e, CultureInfo.InvariantCulture)) : s.Skip(1);
            return new string(a.Concat(b).ToArray());
        }

        /// <summary>
        /// 改行文字を空白に置き換える
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string NewLineToSpace(this string s)
        {
            return _newLineRegex.Replace(s, " ");
        }
    }
}
