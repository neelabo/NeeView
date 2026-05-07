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


        private readonly DisposableCollection _disposables = new();


        private BookmarkFolderList() : base(false, false, Config.Current.Bookmark)
        {
            ApplicationDisposer.Current.Add(this);

            _disposables.Add(Config.Current.Bookmark.SubscribePropertyChanged(nameof(BookmarkConfig.IsSyncBookshelfEnabled), (s, e) =>
            {
                OnPropertyChanged(nameof(IsSyncBookshelfEnabled));
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.FolderSortOrder), async (s, e) =>
            {
                await RefreshAsync(true, true);
            }));

            this.SearchBoxModel = new SearchBoxModel(new BookmarkSearchBoxComponent(this));
        }


        public override QueryPath RootPath => new QueryPath(QueryScheme.Bookmark);

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


        public override bool CanMoveToParent()
        {
            if (_disposedValue) return false;

            var parentQuery = FolderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return parentQuery.Scheme == QueryScheme.Bookmark;
        }

        public override async Task SyncAsync()
        {
            if (_disposedValue) return;

            if (!await SyncBookAsync())
            {
                await SyncCurrentAsync();
            }

            if (Place != null)
            {
                BookmarkPanel.Current.FolderTreeModel?.SyncBookmarkFolder(Place.SimplePath, true);
            }
        }

        private async Task<bool> SyncBookAsync()
        {
            var book = BookHub.Current?.GetCurrentBook();
            var address = book?.Path;

            if (address != null)
            {
                // TODO: Queryの求め方はこれでいいのか？
                var path = new QueryPath(address);

                // ブックマークが存在するパスを求める。
                var node = BookmarkCollectionService.FindBookmark(path, FolderCollection);
                if (node is not null)
                {
                    var parent = node.CreateQuery().GetParent();

                    SetDirty(); // 強制更新
                    await SetPlaceAsync(parent, new FolderItemPosition(path), FolderSetPlaceOption.Focus);

                    RaiseSelectedItemChanged(true);
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> SyncCurrentAsync()
        {
            if (Place != null)
            {
                SetDirty(); // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.Focus);

                RaiseSelectedItemChanged(true);
                return true;
            }

            return false;
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

        public async Task DeleteInvalidBookmark()
        {
            await BookmarkCollectionService.DeleteInvalidBookmark(CancellationToken.None);
        }

        public TreeListNode<IBookmarkEntry>? GetBookmarkPlace()
        {
            return (FolderCollection as BookmarkFolderCollection)?.BookmarkPlace;
        }

        public void SaveLastFolderPath()
        {
            if (Config.Current.StartUp.IsOpenLastBookmarkFolder && Place is not null)
            {
                Config.Current.StartUp.LastBookmarkFolder = new BookshelfFolderMemento(Place, SelectedItem?.TargetPath, FolderCollection?.FolderParameter);
            }
            else
            {
                Config.Current.StartUp.LastBookmarkFolder = null;
            }
        }

        #region IDisposable support

        private bool _disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
