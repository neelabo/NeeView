using NeeLaboratory.Collection;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;
using NeeLaboratory.Linq;
using NeeView.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public partial class BookPageCollection : BindableBase, IReadOnlyList<Page>, IEnumerable<Page>, IDisposable
    {
        private readonly PageThumbnailPool _thumbnailPool = new();

        private List<Archive> _sourceArchives;
        private List<Page> _sourcePages;
        private PageSortMode _sortMode = PageSortMode.Entry;
        private int _sortSeed;
        private string _searchKeyword = "";
        private CancellationTokenSource? _sortCancellationTokenSource;
        private readonly Page _emptyPage;
        private bool _disposedValue = false;
        private readonly Searcher _searcher;
        private MultiMap<string, Page>? _pageTargetMap;


        public BookPageCollection(List<Page> pages)
        {
            _emptyPage = CreateEmptyPage();

            var searchContext = new SearchContext()
                .AddProfile(new DateSearchProfile())
                .AddProfile(new SizeSearchProfile())
                .AddProfile(new PageSearchProfile());
            _searcher = new Searcher(searchContext);

            _sourcePages = pages;
            Pages = pages;

            PageMap = _sourcePages.ToMultiMap(e => e.EntryFullName, e => e);

            foreach (var page in _sourcePages)
            {
                AttachPage(page);
            }

            for (int i = 0; i < _sourcePages.Count; ++i)
            {
                _sourcePages[i].EntryIndex = i;
            }

            _sourceArchives = Pages
                .Select(e => e.ArchiveEntry.Archive)
                .Distinct()
                .WhereNotNull()
                .ToList();
        }

        private void AttachPage(Page page)
        {
            page.Thumbnail.Touched += Thumbnail_Touched;
        }

        private void DetachPage(Page page)
        {
            page.Thumbnail.Touched -= Thumbnail_Touched;
        }


        [Subscribable]
        public event EventHandler? PagesSorting;

        [Subscribable]
        public event EventHandler? PagesSorted;

        // ファイル削除された
        [Subscribable]
        public event EventHandler<PageRemovedEventArgs>? PageRemoved;


        public List<Archive> SourceArchives => _sourceArchives;

        public List<Page> Pages { get; private set; }

        public MultiMap<string, Page> PageMap { get; private set; }

        public MultiMap<string, Page> PageTargetMap
        {
            get
            {
                if (_pageTargetMap is null)
                {
                    _pageTargetMap = _sourcePages.ToMultiMap(e => e.ArchiveEntry.SystemPath, e => e);
                }
                return _pageTargetMap;
            }
        }

        /// <summary>
        /// ソートモード
        /// </summary>
        public PageSortMode SortMode
        {
            get => _sortMode;
            set
            {
                if (SetProperty(ref _sortMode, value))
                {
                    UpdatePagesAsync();
                }
            }
        }

        public int SortSeed
        {
            get => _sortMode == PageSortMode.Random ? _sortSeed : 0;
        }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    UpdatePagesAsync();
                }
            }
        }


        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="sortMode">ソートモード</param>
        /// <param name="searchKeyword">検索キーワード</param>
        /// <param name="token"></param>
        public void Initialize(PageSortMode sortMode, int sortSeed, string searchKeyword, CancellationToken token)
        {
            _sortMode = sortMode;
            _sortSeed = sortSeed;
            _searchKeyword = searchKeyword;
            UpdatePages(_sortMode, _sortSeed, _searchKeyword, token);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ResetPropertyChanged();
                    this.PageRemoved = null;
                    this.PagesSorting = null;
                    this.PagesSorted = null;

                    _sortCancellationTokenSource?.Cancel();
                    _sortCancellationTokenSource?.Dispose();

                    foreach (var page in _sourcePages)
                    {
                        DetachPage(page);
                        page.Dispose();
                    }

                    //Pages.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region IEnumerable<Page> Support

        public IEnumerator<Page> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region List like

        public Page this[int index]
        {
            get
            {
                try
                {
                    return Pages[index];
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            set { Pages[index] = value; }
        }

        public int Count => Pages.Count;

        public int IndexOf(Page page) => Pages.IndexOf(page);

        public Page First() => Pages.First();

        public Page Last() => Pages.Last();

        #endregion

        /// <summary>
        /// 空白ページ作成
        /// </summary>
        private Page CreateEmptyPage()
        {
            var emptyArchiveEntry = new ArchiveEntry(StaticFolderArchive.Default)
            {
                IsEmpty = true,
                RawEntryName = TextResources.GetString("Notice.NoFiles"),
            };
            return new Page(new EmptyPageContent(emptyArchiveEntry, null));
        }

        /// <summary>
        /// サムネイル参照イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumbnail_Touched(object? sender, EventArgs e)
        {
            if (sender is not Thumbnail thumb) return;

            _thumbnailPool.Add(thumb);
        }

        // ページ
        public Page? GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        ////public Page GetPage(string name) => Pages.FirstOrDefault(e => e.EntryName == name);

        public Page? GetPageWithEntryFullName(string name)
        {
            PageMap.TryGetValue(name, out var page);
            return page;
        }

        public Page? GetPageWithTarget(string name)
        {
            PageTargetMap.TryGetValue(name, out var page);
            return page;
        }

        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        public PagePosition FirstPosition() => PagePosition.Zero;

        // 最終ページの場所
        public PagePosition LastPosition() => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : FirstPosition();

        // ページ番号のクランプ
        public int ClampPageNumber(int index)
        {
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            if (index < 0) index = 0;
            return index;
        }

        // ページ場所の有効判定
        public bool IsValidPosition(PagePosition position)
        {
            return (FirstPosition() <= position && position <= LastPosition());
        }

        public bool IsValidIndex(int index)
        {
            return 0 <= index && index < Pages.Count;
        }

        /// <summary>
        /// ページ更新
        /// </summary>
        public void UpdatePagesAsync()
        {
            if (_disposedValue) return;
            if (_sourcePages.Count <= 0) return;

            _sortCancellationTokenSource?.Cancel();
            _sortCancellationTokenSource?.Dispose();
            _sortCancellationTokenSource = new();

            Task.Run(() =>
            {
                try
                {
                    UpdatePages(_sortMode, 0, _searchKeyword, _sortCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // canceled.
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: SortException: {ex}");
                }
            });
        }

        private void UpdatePages(PageSortMode sortMode, int sortSeed, string searchKeyword, CancellationToken token)
        {
            if (_disposedValue) return;
            if (_sourcePages.Count <= 0) return;

            try
            {
                PagesSorting?.Invoke(this, EventArgs.Empty);

                //Debug.WriteLine($"Sort {sortMode} ...");
                var pages = SearchPages(_sourcePages, searchKeyword, token);
                pages = SortPages(pages, sortMode, sortSeed, token);

                if (!pages.Any())
                {
                    pages = new List<Page>() { _emptyPage };
                }

                Pages = pages.ToList();

                //Debug.WriteLine($"Sort {sortMode} done.");

                // ページ ナンバリング
                PagesNumbering();
            }
            // NOTE: LINQ.OrderBy内でのキャンセル例外は InvalidOperationException として返される
            catch (InvalidOperationException ex) when (ex.InnerException is OperationCanceledException canceledException)
            {
                throw canceledException;
            }
            finally
            {
                PagesSorted?.Invoke(this, EventArgs.Empty);
            }
        }

        #region ページの検索

        public IEnumerable<SearchKey> SearchKeywordAnalyze(string keyword)
        {
            return _searcher.Analyze(keyword);
        }

        private IEnumerable<Page> SearchPages(IEnumerable<Page> pages, string keyword, CancellationToken token)
        {
            if (!pages.Any()) return pages;
            if (string.IsNullOrEmpty(keyword)) return pages;

            return _searcher.Search(keyword, pages, token).Cast<Page>();
        }

        #endregion

        #region ページの並び替え

        private IEnumerable<Page> SortPages(IEnumerable<Page> pages, PageSortMode sortMode, int sortSeed, CancellationToken token)
        {
            if (_disposedValue) return pages;
            if (!pages.Any()) return pages;

            IOrderedEnumerable<Page> orderSource = Config.Current.Book.FolderSortOrder switch
            {
                FolderSortOrder.First
                    => pages.OrderBy(e => e.PageType),
                FolderSortOrder.Last
                    => pages.OrderByDescending(e => e.PageType),
                _
                    => pages.OrderBy(e => 0),
            };

            switch (sortMode)
            {
                case PageSortMode.FileName:
                    pages = orderSource.ThenBy(e => e, new ComparerFileName(token));
                    break;
                case PageSortMode.FileNameDescending:
                    pages = orderSource.ThenByDescending(e => e, new ComparerFileName(token));
                    break;
                case PageSortMode.TimeStamp:
                    pages = orderSource.ThenBy(e => e.ArchiveEntry.LastWriteTime).ThenBy(e => e, new ComparerFileName(token));
                    break;
                case PageSortMode.TimeStampDescending:
                    pages = orderSource.ThenByDescending(e => e.ArchiveEntry.LastWriteTime).ThenBy(e => e, new ComparerFileName(token));
                    break;
                case PageSortMode.Size:
                    pages = orderSource.ThenBy(e => e.ArchiveEntry.Length).ThenBy(e => e, new ComparerFileName(token));
                    break;
                case PageSortMode.SizeDescending:
                    pages = orderSource.ThenByDescending(e => e.ArchiveEntry.Length).ThenBy(e => e, new ComparerFileName(token));
                    break;
                case PageSortMode.Random:
                    _sortSeed = sortSeed != 0 ? sortSeed : Random.Shared.Next(1, int.MaxValue);
                    var random = new Random(_sortSeed);
                    pages = orderSource.ThenBy(e => random.Next());
                    break;
                case PageSortMode.Entry:
                    pages = pages.OrderBy(e => e.EntryIndex);
                    break;
                case PageSortMode.EntryDescending:
                    pages = pages.OrderByDescending(e => e.EntryIndex);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return pages;
        }

        /// <summary>
        /// ページ番号設定
        /// </summary>
        private void PagesNumbering()
        {
            for (int i = 0; i < Pages.Count; ++i)
            {
                Pages[i].Index = i;
            }
        }

        /// <summary>
        /// ファイル名ソート用比較クラス
        /// </summary>
        private class ComparerFileName : IComparer<Page>
        {
            private readonly CancellationToken _token;

            public ComparerFileName(CancellationToken token)
            {
                _token = token;
            }

            public int Compare(Page? x, Page? y)
            {
                _token.ThrowIfCancellationRequested();

                if (x is null) return (y is null) ? 0 : -1;
                if (y is null) return 1;

                var xName = x.GetEntryNameTokens();
                var yName = y.GetEntryNameTokens();

                var limit = Math.Min(xName.Length, yName.Length);
                for (int i = 0; i < limit; ++i)
                {
                    if (xName[i] != yName[i])
                    {
                        return NaturalSort.Compare(xName[i], yName[i]);
                    }
                }

                return xName.Length - yName.Length;
            }
        }

        #endregion

        #region ページの削除

        // ページの削除
        // NOTE: 実際にはページ削除時にブック再読み込みを行うため、この処理は使用されない
        public void Remove(List<Page> pages)
        {
            if (_disposedValue) return;
            if (Pages.Count <= 0) return;
            if (pages == null) return;

            var removes = pages.Where(e => Pages.Contains(e)).ToList();
            if (removes.Count <= 0) return;

            foreach (var page in removes)
            {
                _sourcePages.Remove(page);
                Pages.Remove(page);
                PageMap.Remove(page.EntryFullName, page);
                DetachPage(page);
                page.Dispose();
            }

            PagesNumbering();

            PageRemoved?.Invoke(this, new PageRemovedEventArgs(removes));
        }

        // 近くの有効なページを取得
        public Page? GetValidPage(Page? page)
        {
            var index = page != null ? page.Index : 0;
            var answer = Pages.Skip(index).Concat(Pages.Take(index).Reverse()).FirstOrDefault(e => !e.IsDeleted);
            return answer;
        }

        #endregion

        #region ページリスト用現在ページ表示フラグ

        // 表示中ページ
        private List<Page> _viewPages = new();

        /// <summary>
        /// 表示中ページフラグ更新
        /// </summary>
        public void SetViewPageFlag(List<Page> viewPages)
        {
            var hidePages = _viewPages.Except(viewPages).ToList();

            foreach (var page in viewPages)
            {
                page.IsVisible = true;
            }

            foreach (var page in hidePages)
            {
                page.IsVisible = false;
            }

            _viewPages = viewPages.ToList();
        }

        #endregion

        #region フォルダーの先頭ページを取得

        public int GetNextFolderIndex(int start)
        {
            if (Pages.Count == 0 || !SortMode.IsFileNameCategory() || start < 0)
            {
                return -1;
            }

            string currentFolder = LoosePath.GetDirectoryName(Pages[start].EntryName);

            for (int index = start + 1; index < Pages.Count; ++index)
            {
                var folder = LoosePath.GetDirectoryName(Pages[index].EntryName);
                if (currentFolder != folder)
                {
                    return index;
                }
            }

            return -1;
        }

        public int GetPrevFolderIndex(int start)
        {
            if (Pages.Count == 0 || !SortMode.IsFileNameCategory() || start < 0)
            {
                return -1;
            }

            if (start == 0)
            {
                return -1;
            }

            string currentFolder = LoosePath.GetDirectoryName(Pages[start - 1].EntryName);

            for (int index = start - 1; index > 0; --index)
            {
                var folder = LoosePath.GetDirectoryName(Pages[index - 1].EntryName);
                if (currentFolder != folder)
                {
                    return index;
                }
            }

            return 0;
        }

        #endregion
    }
}
