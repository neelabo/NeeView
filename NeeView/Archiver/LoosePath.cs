using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ファイルシステム規約に依存しないパス文字列ユーティリティ
    /// ファイル名に使用できない文字を含んだパスの解析用
    /// </summary>
    public static partial class LoosePath
    {
        public const char DefaultSeparator = '\\';
        public static readonly char[] Separators = new char[] { '\\', '/' };

        public static readonly char[] AsciiSpaces = new char[] {
            '\u0009',  // CHARACTER TABULATION
            '\u000A',  // LINE FEED
            '\u000B',  // LINE TABULATION
            '\u000C',  // FORM FEED
            '\u000D',  // CARRIAGE RETURN
            '\u0020',  // SPACE
        };

        [GeneratedRegex(@"^[a-zA-Z]{2,}:$")]
        private static partial Regex _schemeRegex { get; }


        /// <summary>
        /// 末尾のセパレート記号を削除。
        /// ルート(C:\)の場合は削除しない
        /// </summary>
        public static string TrimEnd(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            s = s.TrimEnd();
            if (Separators.Contains(s.Last()))
            {
                s = s.TrimEnd(Separators);
                if (s.Last() == ':') s += "\\";
            }

            return s;
        }

        /// <summary>
        /// ディレクトリ名用に、終端にセパレート記号を付加する
        /// </summary>
        public static string TrimDirectoryEnd(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.TrimEnd().TrimEnd(Separators) + '\\';
        }

        /// <summary>
        /// 終端にセパレート記号がある？
        /// </summary>
        public static bool IsDirectoryEnd(string? s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return Separators.Contains(s.Last());
        }

        //
        public static string[] Split(string? s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<string>();
            var parts = s.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && (s.StartsWith("\\\\", StringComparison.Ordinal) || s.StartsWith("//", StringComparison.Ordinal)))
            {
                return parts.Skip(1).Prepend("\\\\" + parts.First()).ToArray();
            }
            else
            {
                return parts;
            }
        }

        //
        public static string GetFileName(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        // place部をディレクトリーとみなしたファイル名取得
        public static string GetFileName(string? s, string? place)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (string.IsNullOrEmpty(place)) return s;
            if (string.Compare(s, 0, place, 0, place.Length, StringComparison.OrdinalIgnoreCase) != 0) throw new ArgumentException("s not contain place");
            return s[place.Length..].TrimStart(Separators);
        }

        public static string GetFileNameWithoutExtension(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var filename = GetFileName(s);
            var ext = GetExtension(s);
            if (string.IsNullOrEmpty(ext))
            {
                return filename;
            }
            else
            {
                return filename[..^ext.Length];
            }
        }


        //
        public static string GetPathRoot(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var parts = s.Split(Separators, 2);
            return parts.First();
        }

        //
        public static string GetDirectoryName(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var parts = s.Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count <= 1) return "";

            parts.RemoveAt(parts.Count - 1);
            var path = GetHeadSeparators(s) + string.Join("\\", parts);
            if (parts.Count == 1 && path.Last() == ':') path += "\\";

            return path;
        }

        //
        public static string GetExtension(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            string fileName = GetFileName(s);
            int index = fileName.LastIndexOf('.');

            return (index >= 0) ? fileName[index..].ToLowerInvariant() : "";
        }

        //
        public static string ChopExtension(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var ext = GetExtension(s);
            if (string.IsNullOrEmpty(s)) return s;

            return s[..^ext.Length];
        }

        public static string Combine(string? s1, string? s2, char separator = DefaultSeparator)
        {
            if (string.IsNullOrEmpty(s1))
                return s2 ?? "";
            else if (string.IsNullOrEmpty(s2))
                return s1;
            else if (_schemeRegex.IsMatch(s1))
                return s1 + s2;
            else
                return s1.TrimEnd(Separators) + separator + s2.TrimStart(Separators);
        }

        public static string Combine(params IEnumerable<string?> tokens)
        {
            if (!tokens.Any())
            {
                return "";
            }

            var hasStartSeparator = tokens.Last()?.StartsWith(DefaultSeparator) ?? false;
            var hasEndSeparator = tokens.Last()?.EndsWith(DefaultSeparator) ?? false;
            var items = tokens.Select(e => e?.Trim(Separators)).WhereNotNull().ToList();

            if (items.Count == 0)
            {
                return "";
            }

            if (!hasStartSeparator && _schemeRegex.IsMatch(items[0]))
            {
                var s = items[0]
                    + string.Join(DefaultSeparator, items.Skip(1))
                    + (hasEndSeparator ? DefaultSeparator.ToString() : "");
                return s;
            }
            else
            {
                var s = (hasStartSeparator ? DefaultSeparator.ToString() : "")
                    + string.Join(DefaultSeparator, items)
                    + (hasEndSeparator ? DefaultSeparator.ToString() : "");
                return s;
            }
        }

        // ファイル名として使えない文字を置換
        public static string ValidFileName(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            string valid = s;
            char[] invalids = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in invalids)
            {
                valid = valid.Replace(c, '_');
            }
            return valid;
        }

        // パスとして使えない文字を置換
        // セパレータは標準化されます
        public static string ValidPath(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var tokens = s.Split(Separators);
            var valid = string.Join(DefaultSeparator, tokens.Select(e => ValidFileName(e)));
            return valid;
        }

        // セパレータ標準化
        public static string NormalizeSeparator(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            return s.Replace('/', '\\');
        }

        // UNC判定
        public static bool IsUnc(string? s)
        {
            var head = GetHeadSeparators(s);
            return head.Length == 2;
        }

        // パス先頭にあるセパレータ部を取得
        private static string GetHeadSeparators(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var slashCount = 0;
            foreach (var c in s)
            {
                if (c == '\\' || c == '/')
                {
                    slashCount++;
                }
                else
                {
                    break;
                }
            }

            return slashCount > 0 ? new string('\\', slashCount) : "";
        }

        // 表示用のファイル名生成
        public static string GetDisplayName(string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "PC";
            }
            else
            {
                // ドライブ名なら終端に「￥」を付ける
                var name = LoosePath.GetFileName(s);
                if (s.Length <= 3 && name.Length == 2 && name[1] == ':')
                {
                    name += '\\';
                }
                return name;
            }
        }

        /// <summary>
        /// パスを "FooBar (C:\Parent)" 形式にする
        /// </summary>
        public static string GetPlaceName(string? s)
        {
            var name = GetFileName(s);
            var parent = GetDirectoryName(s);

            if (string.IsNullOrEmpty(parent))
            {
                return name;
            }
            else
            {
                return name + " (" + parent + ")";
            }
        }

        /// <summary>
        /// ファイル名変更。ディレクトリはそのまま。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Rename(string path, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return path;
            return Combine(GetDirectoryName(path), name);
        }
    }
}
