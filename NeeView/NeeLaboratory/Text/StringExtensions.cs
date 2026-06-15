using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeLaboratory.Text
{
    public static partial class StringExtensions
    {
        [GeneratedRegex(@"[\r\n]+")]
        private static partial Regex _newLineRegex { get; }

        private static readonly SearchValues<char> _lineBreaks = SearchValues.Create(['\r', '\n']);


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

        /// <summary>
        /// "\a" のような文字列を、エスケープされているかどうかを含めて列挙する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IEnumerable<EscapedChar> EscapeStringWalker(this string s)
        {
            bool isEscaped = false;
            foreach (var c in s)
            {
                if (c == '\\' && !isEscaped)
                {
                    isEscaped = true;
                    continue;
                }
                yield return new(c, isEscaped);
                isEscaped = false;
            }
        }

        /// <summary>
        /// 先頭行を習得
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FirstLine(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            using (var reader = new StringReader(s))
            {
                return reader.ReadLine() ?? string.Empty;
            }
        }

        /// <summary>
        /// 改行を削除して１行に変換する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToOneLine(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            ReadOnlySpan<char> source = s.AsSpan();

            int firstLook = source.IndexOfAny(_lineBreaks);

            if (firstLook == -1)
            {
                return s;
            }

            // CRLF ペアが出力長を 1 つ縮めるので、CRLF の数を数えて確定長を計算する
            int crlfCount = 0;
            for (int i = firstLook; i + 1 < s.Length; i++)
            {
                if (s[i] == '\r' && s[i + 1] == '\n') crlfCount++;
            }
            int exactLength = s.Length - crlfCount;

            return string.Create(exactLength, (s, firstLook), (dest, state) =>
            {
                var src = state.s;
                int first = state.Item2;

                // 最初の部分を一括コピー
                src.AsSpan(0, first).CopyTo(dest);

                const char replaceChar = ' ';
                int destIndex = first;
                for (int srcIndex = first; srcIndex < src.Length; srcIndex++)
                {
                    char c = src[srcIndex];
                    if (c == '\r')
                    {
                        if (srcIndex + 1 < src.Length && src[srcIndex + 1] == '\n') srcIndex++;
                        dest[destIndex++] = replaceChar;
                    }
                    else if (c == '\n')
                    {
                        dest[destIndex++] = replaceChar;
                    }
                    else
                    {
                        dest[destIndex++] = c;
                    }
                }
            });
        }

        /// <summary>
        /// 相互置き換え
        /// </summary>
        /// <param name="s"></param>
        /// <param name="wordA">置換文字列A</param>
        /// <param name="wordB">置換文字列B</param>
        /// <param name="placeHolder">保持用ダミー文字列。絶対にマッチしない文字列でなければいけない</param>
        /// <returns></returns>
        public static string ReplaceSwap(this string s, string wordA, string wordB, string? placeHolder = null)
        {
            if (string.IsNullOrEmpty(s)) return s;

            placeHolder ??= Guid.NewGuid().ToString();

            var pattern = Regex.Escape(wordA) + '|' + Regex.Escape(wordB);
            var replaced = Regex.Replace(s, pattern, m => m.Value == wordA ? placeHolder : wordA);
            replaced = replaced.Replace(placeHolder, wordB);

            return replaced;
        }

    }


    /// <summary>
    /// 文字と、その文字がエスケープされているかどうかを表す構造体
    /// </summary>
    public readonly record struct EscapedChar(char Char, bool IsEscaped)
    {
        public EscapedChar(char c) : this(c, false)
        {
        }
    }
}
