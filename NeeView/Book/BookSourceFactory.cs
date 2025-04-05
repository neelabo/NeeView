using NeeView.Collections.Generic;
using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class BookSourceFactory
    {
        // 本読み込み
        public static async Task<BookSource> CreateAsync(BookAddress address, BookCreateSetting setting, CancellationToken token)
        {
            // ページ生成
            var archiveEntryCollection = CreateArchiveEntryCollection(address.TargetPath.SimplePath, setting.IsRecursiveFolder, setting.ArchiveRecursiveMode, setting.IsIgnoreCache, setting.ArchiveHint);
            var bookMemoryService = new BookMemoryService();
            var pages = await CreatePageCollection(archiveEntryCollection, setting.BookPageCollectMode, new PageContentFactory(bookMemoryService, true), setting.Decrypt, token);

            // 再起判定は通常のディレクトリーのみ適用
            var canAutoRecursive = System.IO.Directory.Exists(address.TargetPath.SimplePath);

            // 再帰判定用サブフォルダー数カウント
            int subFolderCount = 0;
            if (canAutoRecursive && archiveEntryCollection.Mode != ArchiveEntryCollectionMode.IncludeSubArchives && !pages.Where(e => e.PageType == PageType.File).Any())
            {
                var entries = await archiveEntryCollection.GetEntriesWhereBookAsync(setting.Decrypt, token);
                subFolderCount = entries.Count;
            }

            // prefix設定
            SetPagePrefix(pages);

            // Validate sort mode
            var sortMode = ValidatePageSortMode(setting.SortMode, archiveEntryCollection);
            var searchKeyword = address.TargetPath.Search ?? "";

            var pageCollection = new BookPageCollection(pages);
            pageCollection.Initialize(sortMode, searchKeyword, token);
            var book = new BookSource(archiveEntryCollection, pageCollection, bookMemoryService);
            book.SubFolderCount = subFolderCount;

            return book;
        }

        private static PageSortMode ValidatePageSortMode(PageSortMode sortMode, ArchiveEntryCollection archiveEntryCollection)
        {
            // プレイリストならば登録順有効、それ以外は無効
            var isPlaylist = archiveEntryCollection?.Archive is PlaylistArchive;
            var pageSortModeClass = isPlaylist ? PageSortModeClass.WithEntry : PageSortModeClass.Normal;
            return pageSortModeClass.ValidatePageSortMode(sortMode);
        }

        private static ArchiveEntryCollection CreateArchiveEntryCollection(string place, bool isRecursive, ArchiveEntryCollectionMode archiveRecursiveMode, bool isIgnoreCache, ArchiveHint archiveHint)
        {
            var collectMode = isRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.CurrentDirectory;
            var collectModeIfArchive = isRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : archiveRecursiveMode;
            var collectOption = isIgnoreCache ? ArchiveEntryCollectionOption.IgnoreCache : ArchiveEntryCollectionOption.None;
            return new ArchiveEntryCollection(place, collectMode, collectModeIfArchive, collectOption, archiveHint);
        }

        /// <summary>
        /// ページ生成
        /// </summary>
        private static async Task<List<Page>> CreatePageCollection(ArchiveEntryCollection archiveEntryCollection, BookPageCollectMode bookPageCollectMode, PageContentFactory contentFactory, bool decrypt, CancellationToken token)
        {
            List<ArchiveEntryNode> entries = bookPageCollectMode switch
            {
                BookPageCollectMode.Image => await archiveEntryCollection.GetEntriesWhereImageAsync(decrypt, token),
                BookPageCollectMode.ImageAndBook => await archiveEntryCollection.GetEntriesWhereImageAndArchiveAsync(decrypt, token),
                _ => await archiveEntryCollection.GetEntriesWherePageAllAsync(decrypt, token),
            };

            Debug.Assert(entries.Count == 0 || entries[0].RootSystemPath == archiveEntryCollection.Path);

            return entries.Select(e => CreatePage(e, contentFactory, token)).ToList();
        }

        /// <summary>
        /// ページ作成
        /// </summary>
        /// <param name="entryNode">論理パス</param>
        /// <returns></returns>
        private static Page CreatePage(ArchiveEntryNode entryNode, PageContentFactory contentFactory, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return new Page(contentFactory.CreatePageContent(entryNode.ArchiveEntry, token), entryNode);
        }


        /// <summary>
        /// PageのPrefix設定
        /// </summary>
        private static void SetPagePrefix(List<Page> pages)
        {
            // TODO: ページ生成と同時に行うべき?
            var prefix = GetPagesPrefix(pages);
            foreach (var page in pages)
            {
                page.Prefix = prefix;
            }
        }

        // 名前の最長一致文字列取得
        private static string GetPagesPrefix(List<Page> pages)
        {
            if (pages == null || pages.Count == 0) return "";

            string? s = pages[0].EntryName;
            if (s is null) return "";
            foreach (var page in pages)
            {
                s = GetStartsWith(s, page.EntryName);
                if (string.IsNullOrEmpty(s)) break;
            }

            // １ディレクトリだけの場合に表示が消えないようにする
            if (pages.Count == 1)
            {
                s = s.TrimEnd('\\', '/');
            }

            // 最初の区切り記号
            for (int i = s.Length - 1; i >= 0; --i)
            {
                if (s[i] == '\\' || s[i] == '/')
                {
                    return s[..(i + 1)];
                }
            }

            // ヘッダとして認識できなかった
            return "";
        }

        //
        private static string GetStartsWith(string s0, string s1)
        {
            if (s0 == null || s1 == null) return "";

            if (s0.Length > s1.Length)
            {
                (s1, s0) = (s0, s1);
            }

            for (int i = 0; i < s0.Length; ++i)
            {
                if (s0[i] != s1[i])
                {
                    return i > 0 ? s0[..i] : "";
                }
            }

            return s0;
        }

    }

}
