using NeeLaboratory.Linq;
using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace NeeView
{
    public enum QueryScheme
    {
        [AliasName]
        File = 0,

        [AliasName]
        Root,

        [AliasName]
        Bookmark,

        [AliasName]
        QuickAccess,

        [AliasName]
        Script,
    }

    public static class QuerySchemeExtensions
    {
        static readonly Dictionary<QueryScheme, string> _map = new()
        {
            [QueryScheme.File] = "file:",
            [QueryScheme.Root] = "root:",
            [QueryScheme.Bookmark] = "bookmark:",
            [QueryScheme.QuickAccess] = "quickaccess:",
            [QueryScheme.Script] = "script:",
        };

        static Dictionary<QueryScheme, ImageSource>? _imageMap;

        static Dictionary<QueryScheme, ImageSource>? _thumbnailImageMap;


        [MemberNotNull(nameof(_imageMap))]
        private static void InitializeImageMap()
        {
            if (_imageMap != null) return;

            _imageMap = AppDispatcher.Invoke(() =>
            {
                return new Dictionary<QueryScheme, ImageSource>()
                {
                    [QueryScheme.File] = MainWindow.Current.Resources["ic_desktop_windows_24px"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.Root] = MainWindow.Current.Resources["ic_bookshelf"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.Bookmark] = MainWindow.Current.Resources["ic_grade_24px"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.QuickAccess] = MainWindow.Current.Resources["ic_lightning"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.Script] = MainWindow.Current.Resources["ic_javascript_24px"] as ImageSource ?? throw new DirectoryNotFoundException(),
                };
            });
        }

        [MemberNotNull(nameof(_thumbnailImageMap))]
        private static void InitializeThumbnailImageMap()
        {
            if (_thumbnailImageMap != null) return;

            _thumbnailImageMap = AppDispatcher.Invoke(() =>
            {
                return new Dictionary<QueryScheme, ImageSource>()
                {
                    [QueryScheme.File] = MainWindow.Current.Resources["ic_desktop_windows_24px_t"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.Root] = MainWindow.Current.Resources["ic_bookshelf"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.Bookmark] = MainWindow.Current.Resources["ic_grade_24px_t"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.QuickAccess] = MainWindow.Current.Resources["ic_lightning"] as ImageSource ?? throw new DirectoryNotFoundException(),
                    [QueryScheme.Script] = MainWindow.Current.Resources["ic_javascript_24px"] as ImageSource ?? throw new DirectoryNotFoundException(),
                };
            });
        }

        public static string ToSchemeString(this QueryScheme scheme)
        {
            return _map[scheme];
        }

        public static QueryScheme GetScheme(string path)
        {
            return _map.FirstOrDefault(e => path.StartsWith(e.Value, StringComparison.Ordinal)).Key;
        }

        public static ImageSource ToImage(this QueryScheme scheme)
        {
            InitializeImageMap();
            return _imageMap[scheme];
        }

        public static ImageSource ToThumbnailImage(this QueryScheme scheme)
        {
            InitializeThumbnailImageMap();
            return _thumbnailImageMap[scheme];
        }

        public static bool IsMatch(this QueryScheme scheme, string path)
        {
            return path.StartsWith(scheme.ToSchemeString(), StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// パスのクエリパラメータを分解する.
    /// immutable.
    /// </summary>
    public sealed partial record class QueryPath
    {
        [GeneratedRegex(@"^\\*([a-zA-Z]):(.*)$")]
        private static partial Regex _driveRegex { get; }

        private static readonly string _querySearch = "?search=";

        public static QueryPath Empty { get; } = new QueryPath(QueryScheme.Root);


        private string? _search;

        private string[]? _tokens;


        public QueryPath(string? source)
        {
            var rest = source;
            (rest, Search) = TakeQuerySearch(rest);
            (rest, Scheme) = TakeScheme(rest);
            Path = GetValidatePath(rest, Scheme);
        }

        public QueryPath(string? source, string? search)
        {
            var rest = source;
            Search = search;
            (rest, Scheme) = TakeScheme(rest);
            Path = GetValidatePath(rest, Scheme);
        }

        public QueryPath(QueryScheme scheme, string? path, string? search)
        {
            Search = search;
            Scheme = scheme;
            Path = GetValidatePath(path, Scheme);
        }

        public QueryPath(QueryScheme scheme, string? path)
        {
            Search = null;
            Scheme = scheme;
            Path = GetValidatePath(path, Scheme);
        }

        public QueryPath(QueryScheme scheme)
        {
            Search = null;
            Scheme = scheme;
            Path = null;
        }


        public QueryScheme Scheme { get; init; }

        public string? Path { get; init; }

        public string? Search
        {
            get => _search;
            init => _search = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public bool IsEmpty => Path is null;


        /// <summary>
        /// 完全クエリ
        /// </summary>
        public string FullQuery => FullPath + (Search != null ? _querySearch + Search : null);

        /// <summary>
        /// 簡略化したクエリ
        /// </summary>
        public string SimpleQuery => SimplePath + (Search != null ? _querySearch + Search : null);

        /// <summary>
        /// 完全パス
        /// </summary>
        public string FullPath => Scheme.ToSchemeString() + Path;

        /// <summary>
        /// 簡略化したパス
        /// </summary>
        public string SimplePath => Scheme == QueryScheme.File ? Path ?? "" : FullPath;

        public string FileName => LoosePath.GetFileName(Path);

        public string DisplayName => (Path == null) ? Scheme.ToAliasName() : FileName;

        public string DisplayPath => (Path == null) ? Scheme.ToAliasName() : SimplePath;

        public string[] Tokens => _tokens ??= CreateTokens();


        private static (string? RestSource, string? SearchWord) TakeQuerySearch(string? source)
        {
            if (source != null)
            {
                var index = source.IndexOf(_querySearch, StringComparison.Ordinal);
                if (index >= 0)
                {
                    return (source[..index], source[(index + _querySearch.Length)..]);
                }
            }

            return (source, null);
        }

        private static (string? RestSource, QueryScheme Scheme) TakeScheme(string? source)
        {
            if (source != null)
            {
                var scheme = QuerySchemeExtensions.GetScheme(source);
                var schemeString = scheme.ToSchemeString();
                if (source.StartsWith(schemeString, StringComparison.Ordinal))
                {
                    return (source[schemeString.Length..], scheme);
                }
            }
            return (source, QueryScheme.File);
        }

        public QueryPath GetParent()
        {
            if (Path == null)
            {
                return QueryPath.Empty;
            }

            var parent = LoosePath.GetDirectoryName(Path);
            return new QueryPath(this.Scheme, parent, null);
        }

        public bool Include(QueryPath target)
        {
            var pathX = this.FullPath;
            var pathY = target.FullPath;

            var lengthX = pathX.Length;
            var lengthY = pathY.Length;

            if (lengthX > lengthY)
            {
                return false;
            }
            else if (lengthX == lengthY)
            {
                return pathX == pathY;
            }
            else
            {
                return pathY.StartsWith(pathX, StringComparison.Ordinal) && pathY[lengthX] == '\\';
            }
        }

        public bool IsRoot(QueryScheme scheme)
        {
            return Scheme == scheme && Path == null && Search == null;
        }

        public static QueryPath Parse(string? path)
        {
            return new QueryPath(path);
        }

        public override string ToString()
        {
            return FullQuery;
        }

        private static string? GetValidatePath(string? source, QueryScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var s = LoosePath.NormalizeSeparator(source).Trim(LoosePath.AsciiSpaces).TrimEnd('\\');

            if (scheme == QueryScheme.File)
            {
                // fix drive name
                var match = _driveRegex.Match(s);
                if (match.Success)
                {
                    s = match.Groups[1].Value.ToUpperInvariant() + ':' + match.Groups[2].Value;
                    // fix end separator of drive root
                    if (s.Length == 2 && s[1] == ':')
                    {
                        s += '\\';
                    }
                }
            }
            else
            {
                s = s.TrimStart('\\');
            }

            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        /// <summary>
        /// 正規化
        /// </summary>
        /// <remarks>
        /// ファイル存在チェックを行うので重め
        /// </remarks>
        /// <returns></returns>
        public QueryPath Normalize()
        {
            if (Scheme == QueryScheme.File)
            {
                return new QueryPath(QueryScheme.File, FileIO.GetNormalizedPath(SimplePath), Search);
            }
            return this;
        }

        /// <summary>
        /// ショートカットの解決
        /// </summary>
        /// <remarks>
        /// ショートカットならば実体のパスに変換する。
        /// スクリプトスキームならばファイルパスに変換する。
        /// 他のスキームは非対応
        /// </remarks>
        /// <returns></returns>
        public QueryPath ResolvePath()
        {
            if (Scheme == QueryScheme.File)
            {
                var path = SimplePath;
                if (FileShortcut.IsShortcut(path) && File.Exists(path))
                {
                    var shortcut = new FileShortcut(path);
                    if (shortcut.TryGetTargetPath(out var target))
                    {
                        path = FileIO.GetNormalizedPath(target);
                    }
                    return new QueryPath(QueryScheme.File, path, Search);
                }
            }
            else if (Scheme == QueryScheme.Script)
            {
                var path = LoosePath.Combine(Config.Current.Script.ScriptFolder, Path);
                var query = new QueryPath(QueryScheme.File, FileIO.GetNormalizedPath(path), Search);
                return query.ResolvePath();
            }
            return this;
        }

        /// <summary>
        /// スキームを含むフルパスをトークン単位に分割する
        /// </summary>
        /// <returns></returns>
        private string[] CreateTokens()
        {
            // NOTE: Path の先頭に区切り記号が来ることは想定していません。
            var scheme = Scheme.ToSchemeString();
            var tokens = string.IsNullOrEmpty(Path) ? [] : Path.Split(LoosePath.DefaultSeparator);
            return tokens.Prepend(scheme).WhereNotNull().ToArray();
        }

        /// <summary>
        /// トークン列から部分パスを取得する
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public QueryPath Substring(int startIndex, int length)
        {
            return new QueryPath(LoosePath.Combine(Tokens.Skip(startIndex).Take(length)));
        }
    }
}
