using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// BookmarkFolderList
    /// </summary>
    public class BookmarkFolderList : FolderList, IDisposable
    {
        static BookmarkFolderList() => Current = new BookmarkFolderList();
        public static BookmarkFolderList Current { get; }


        private CancellationTokenSource? _removeUnlinkedCancellationTokenSource;
        private readonly DisposableCollection _disposables = new();


        private BookmarkFolderList() : base(false, false, Config.Current.Bookmark)
        {
            ApplicationDisposer.Current.Add(this);

            _disposables.Add(Config.Current.Bookmark.SubscribePropertyChanged(nameof(BookmarkConfig.IsSyncBookshelfEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsSyncBookshelfEnabled));
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.FolderSortOrder), async (s, e) =>
            {
                await RefreshAsync(true, true);
            }));

            this.SearchBoxModel = new SearchBoxModel(new BookmarkSearchBoxComponent(this));
        }


        public override bool IsSyncBookshelfEnabled
        {
            get => Config.Current.Bookmark.IsSyncBookshelfEnabled;
            set => Config.Current.Bookmark.IsSyncBookshelfEnabled = value;
        }

        public override bool IsSearchIncludeSubdirectories
        {
            get => Config.Current.Bookmark.IsSearchIncludeSubdirectories;
            set => Config.Current.Bookmark.IsSearchIncludeSubdirectories = value;
        }

        public override bool SyncBookOnRename => false;


        public void UpdateItems()
        {
            if (_disposedValue) return;

            if (FolderCollection == null)
            {
                RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.None);
            }
        }

        public override bool CanMoveToParent()
        {
            if (_disposedValue) return false;

            var parentQuery = FolderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return parentQuery.Scheme == QueryScheme.Bookmark;
        }

        public override void Sync()
        {
        }

        public override QueryPath GetFixedHome()
        {
            return new QueryPath(QueryScheme.Bookmark, null, null);
        }

        protected override bool CheckScheme(QueryPath query)
        {
            if (query.Scheme != QueryScheme.Bookmark) throw new NotSupportedException($"need scheme \"{QueryScheme.Bookmark.ToSchemeString()}\"");
            return true;
        }

        public async ValueTask DeleteInvalidBookmark()
        {
            // 直前の命令はキャンセル
            _removeUnlinkedCancellationTokenSource?.Cancel();
            _removeUnlinkedCancellationTokenSource = new CancellationTokenSource();
            await BookmarkCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCancellationTokenSource.Token);
        }

        public TreeListNode<IBookmarkEntry>? GetBookmarkPlace()
        {
            return (FolderCollection as BookmarkFolderCollection)?.BookmarkPlace;
        }

        #region IDisposable support

        private bool _disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _removeUnlinkedCancellationTokenSource?.Cancel();
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        #endregion IDisposable support



        /// <summary>
        /// 検索ボックスコンポーネント
        /// </summary>
        public class BookmarkSearchBoxComponent : ISearchBoxComponent
        {
            private readonly BookmarkFolderList _self;

            public BookmarkSearchBoxComponent(BookmarkFolderList self)
            {
                _self = self;
            }

            public HistoryStringCollection? History => BookHistoryCollection.Current.BookmarkSearchHistory;

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.RequestSearchPlace(keyword, false);
        }
    }
}
