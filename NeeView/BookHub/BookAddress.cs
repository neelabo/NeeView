using NeeView.Properties;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookAddressException : Exception
    {
        public BookAddressException(string message) : base(message)
        {
        }

        public BookAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// アーカイブパスに対応したブックアドレス
    /// </summary>
    public class BookAddress
    {
        public BookAddress()
        {
        }

        /// <summary>
        /// 開始ページ名
        /// </summary>
        public string? EntryName { get; set; }

        /// <summary>
        /// ブックのアドレス
        /// </summary>
        public QueryPath TargetPath { get; set; } = QueryPath.None;

        /// <summary>
        /// ソースアドレス。ショートカットファイルとか
        /// </summary>
        public QueryPath? SourcePath { get; set; }

        /// <summary>
        /// ブックのあるフォルダー
        /// </summary>
        public QueryPath Place { get; set; } = QueryPath.None;

        /// <summary>
        /// EntryName の基準となるパス
        /// </summary>
        public QueryPath ArchivePath { get; set; } = QueryPath.None;

        /// <summary>
        /// ページを含めたアーカイブパス
        /// </summary>
        public string SystemPath => LoosePath.Combine(ArchivePath?.SimplePath, EntryName);



        /// <summary>
        /// BookAddress生成
        /// </summary>
        ///  <param name="query">入力パス</param>
        /// <param name="entryName">開始ページ名</param>
        /// <param name="option"></param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>生成したインスタンス</returns>
        // TODO: 非同期ではパラメータ生成のみを行い、BookAddressコンストラクタにそれを渡すようにして初期化する
        public static async ValueTask<BookAddress> CreateAsync(QueryPath query, QueryPath sourceQuery, string? entryName, ArchiveEntryCollectionMode mode, BookLoadOption option, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var address = new BookAddress();
            await address.ConstructAsync(query, sourceQuery, entryName, mode, option, token);
            return address;
        }

        /// <summary>
        /// 初期化。
        /// アーカイブ展開等を含むため、非同期処理。
        /// </summary>
        private async ValueTask ConstructAsync(QueryPath query, QueryPath sourceQuery, string? entryName, ArchiveEntryCollectionMode mode, BookLoadOption option, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            this.SourcePath = sourceQuery ?? query;
            var search = query.Search;

            // ブックマークは実体のパスへ
            if (query.Scheme == QueryScheme.Bookmark)
            {
                var node = BookmarkCollection.Current.FindNode(query);
                if (node is null) throw new InvalidOperationException();
                switch (node.Value)
                {
                    case Bookmark bookmark:
                        query = new QueryPath(bookmark.Path, search);
                        break;
                    case BookmarkFolder folder:
                        throw new BookAddressException(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.CannotOpenBookmarkFolder"), query.SimplePath));
                }
            }

            // アーカイブエントリを取得
            var entry = await ArchiveEntryUtility.CreateAsync(query.SimplePath, ArchiveHint.None, true, token);

            // ページ名が指定されているなら入力そのまま
            if (entryName != null)
            {
                this.TargetPath = query;
                this.ArchivePath = query;
                this.EntryName = entryName;
            }
            // 検索オプションが指定されてたらブック
            else if (search != null)
            {
                this.TargetPath = query;
                this.ArchivePath = query;
                this.EntryName = null;
            }
            // パスはブック
            else if (entry.IsBook() || option.HasFlag(BookLoadOption.IsBook))
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsPage));
                this.TargetPath = query;
                this.ArchivePath = query;
                this.EntryName = null;
            }
            // パスはページ
            else
            {
                try
                {
                    if (entry.IsFileSystem)
                    {
                        this.TargetPath = query.GetParent();
                        this.ArchivePath = this.TargetPath;
                    }
                    else
                    {
                        switch (mode)
                        {
                            case ArchiveEntryCollectionMode.CurrentDirectory:
                                this.TargetPath = query.GetParent();
                                this.ArchivePath = new QueryPath(entry.Archive.SystemPath);
                                break;
                            case ArchiveEntryCollectionMode.IncludeSubDirectories:
                                this.TargetPath = new QueryPath(entry.Archive.SystemPath);
                                this.ArchivePath = this.TargetPath;
                                break;
                            case ArchiveEntryCollectionMode.IncludeSubArchives:
                                this.TargetPath = new QueryPath(entry.RootArchive.SystemPath);
                                this.ArchivePath = this.TargetPath;
                                break;
                            default:
                                throw new NotSupportedException($"{nameof(ArchiveEntryCollectionMode)}.{mode} is not supported.");
                        }
                    }
                    this.EntryName = GetEntryName(query, this.ArchivePath);
                    entry = await ArchiveEntryUtility.CreateAsync(TargetPath.SimplePath, ArchiveHint.None, true, token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    this.TargetPath = query.GetParent();
                    this.ArchivePath = query.GetParent();
                    this.EntryName = query.FileName;
                    entry = await ArchiveEntryUtility.CreateAsync(TargetPath.SimplePath, ArchiveHint.None, true, token);
                }
            }

            this.Place = GetPlace(entry, mode);
            //Debug.Assert(this.Place != null);
        }

        /// <summary>
        /// エントリのあるフォルダーの場所を取得
        /// </summary>
        private static QueryPath GetPlace(ArchiveEntry entry, ArchiveEntryCollectionMode mode)
        {
            if (entry == null)
            {
                return new QueryPath(QueryScheme.Root);
            }

            if (entry.IsFileSystem)
            {
                return new QueryPath(entry.SystemPath).GetParent();
            }
            else
            {
                if (mode == ArchiveEntryCollectionMode.IncludeSubArchives)
                {
                    return new QueryPath(entry.Archive.RootArchive.SystemPath).GetParent();
                }
                else if (mode == ArchiveEntryCollectionMode.IncludeSubDirectories)
                {
                    if (entry.IsArchive())
                    {
                        return new QueryPath(entry.Archive.SystemPath);
                    }
                    else if (entry.Archive.Parent != null)
                    {
                        return new QueryPath(entry.Archive.Parent.SystemPath);
                    }
                    else
                    {
                        return new QueryPath(entry.Archive.SystemPath).GetParent();
                    }
                }
                else
                {
                    return new QueryPath(entry.SystemPath).GetParent();
                }
            }
        }

        private static string GetEntryName(QueryPath query, QueryPath address)
        {
            if (query is null) throw new ArgumentNullException(nameof(query));
            if (address is null) throw new ArgumentNullException(nameof(address));

            var full = query.SimplePath;
            if (!full.StartsWith(address.SimplePath, StringComparison.Ordinal)) throw new ArgumentException($"{address} is not include entry.", nameof(address));
            return full[address.SimplePath.Length..].TrimStart(LoosePath.Separators);
        }
    }

}
