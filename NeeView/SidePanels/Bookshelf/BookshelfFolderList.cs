using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// BookshelfFolderList
    /// </summary>
    public class BookshelfFolderList : FolderList, IDisposable
    {
        static BookshelfFolderList() => Current = new BookshelfFolderList();
        public static BookshelfFolderList Current { get; }


        private FolderItem? _visibleItem;
        private Regex? _excludeRegex;
        private readonly DisposableCollection _disposables = new();


        private BookshelfFolderList() : base(true, true, Config.Current.Bookshelf)
        {
            History = new BookshelfFolderHistory(this);

            ApplicationDisposer.Current.Add(this);

            _disposables.Add(Config.Current.System.SubscribePropertyChanged(nameof(SystemConfig.IsHiddenFileVisible), async (s, e) =>
            {
                await RefreshAsync(true, true);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.FolderSortOrder), async (s, e) =>
            {
                await RefreshAsync(true, true);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.IsVisibleHistoryMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.IsVisibleBookmarkMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.ExcludePattern), (s, e) =>
            {
                UpdateExcludeRegex();
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.IsSearchIncludeSubdirectories), (s, e) =>
            {
                RequestSearchPlace(true);
            }));

            _disposables.Add(BookOperation.Current.SubscribeBookChanging((s, e) =>
            {
                UpdateVisibleItem(e.Address, false);
            }));

            _disposables.Add(BookOperation.Current.SubscribeBookChanged((s, e) =>
            {
                UpdateVisibleItem(BookOperation.Current.Address, false);
            }));

            _disposables.Add(this.SubscribeCollectionChanged((s, e) =>
            {
                UpdateVisibleItem(BookOperation.Current.Address, true);
                RaisePropertyChanged(nameof(PlacePath));
            }));

            this.SearchBoxModel = new SearchBoxModel(new BookshelfSearchBoxComponent(this));

            UpdateExcludeRegex();
        }


        // フォルダー履歴
        public BookshelfFolderHistory History { get; }

        // 除外パターンの正規表現
        public Regex? ExcludeRegex
        {
            get { return _excludeRegex; }
            set { SetProperty(ref _excludeRegex, value); }
        }

        public override bool IsSearchIncludeSubdirectories
        {
            get => Config.Current.Bookshelf.IsSearchIncludeSubdirectories;
            set => Config.Current.Bookshelf.IsSearchIncludeSubdirectories = value;
        }

        public string? PlacePath
        {
            get { return Place?.SimplePath; }
            set
            {
                if (Place?.SimplePath != value)
                {
                    RequestPlace(new QueryPath(value), null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);
                }
            }
        }


        /// <summary>
        /// 現在ブックマーク更新
        /// </summary>
        private void UpdateVisibleItem(string? path, bool force)
        {
            if (_disposedValue) return;

            if (force && _visibleItem != null)
            {
                _visibleItem.IsVisible = false;
                _visibleItem = null;
            }

            if (_visibleItem != null && _visibleItem.EntityPath.SimplePath == path)
            {
                return;
            }

            var item = FolderCollection != null
                ? FolderCollection.Items.FirstOrDefault(x => x.TargetPath.SimplePath == path) ?? FolderCollection.Items.FirstOrDefault(x => x.EntityPath.SimplePath == path)
                : null;

            if (_visibleItem != item)
            {
                if (_visibleItem != null)
                {
                    _visibleItem.IsVisible = false;
                }
                _visibleItem = item;
                if (_visibleItem != null)
                {
                    _visibleItem.IsVisible = true;
                }
            }
        }

        internal void ToggleVisibleFoldersTree()
        {
            if (_disposedValue) return;

            var command = CommandTable.Current.GetElement("ToggleVisibleFoldersTree");
            if (command.CanExecute(this, CommandArgs.Empty))
            {
                command.Execute(this, CommandArgs.Empty);
            }
        }

        public override QueryPath GetFixedHome()
        {
            var path = new QueryPath(Config.Current.Bookshelf.Home);

            switch (path.Scheme)
            {
                case QueryScheme.Root:
                    return path;

                case QueryScheme.File:
                    if (Directory.Exists(path.SimplePath))
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(GetDefaultHomePath());
                    }

                case QueryScheme.Bookmark:
                    if (BookmarkCollection.Current.FindNode(path.SimplePath)?.Value is BookmarkFolder)
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(QueryScheme.Bookmark, null, null);
                    }

                default:
                    Debug.WriteLine($"Not support yet: {Config.Current.Bookshelf.Home}");
                    return new QueryPath(GetDefaultHomePath());
            }
        }

        public static string GetDefaultHomePath()
        {
            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture))
            {
                return myPicture;
            }

            // 救済措置
            return System.Environment.CurrentDirectory;
        }

        public override async void Sync()
        {
            if (_disposedValue) return;

            var book = BookHub.Current?.GetCurrentBook();
            var address = book?.Path;

            if (address != null)
            {
                // TODO: Queryの求め方はこれでいいのか？
                var path = new QueryPath(address);
                var parent = new QueryPath(book?.Source.GetFolderPlace() ?? LoosePath.GetDirectoryName(address));

                SetDirty(); // 強制更新
                await SetPlaceAsync(parent, new FolderItemPosition(path), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.FileSystem);

                RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                SetDirty(); // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.FileSystem);

                RaiseSelectedItemChanged(true);
            }

            if (Config.Current.Bookshelf.IsSyncFolderTree && Place != null)
            {
                FolderPanel.Current.FolderTreeModel?.SyncDirectory(Place.SimplePath);
            }
        }

        protected override void CloseBookIfNecessary()
        {
            if (_disposedValue) return;

            if (Config.Current.Bookshelf.IsCloseBookWhenMove)
            {
                BookHub.Current.RequestUnload(this, true);
            }
        }

        protected override bool IsCruise()
        {
            return Config.Current.Bookshelf.IsCruise;
        }

        // 除外パターンの正規表現を更新
        private void UpdateExcludeRegex()
        {
            if (_disposedValue) return;

            try
            {
                ExcludeRegex = string.IsNullOrWhiteSpace(Config.Current.Bookshelf.ExcludePattern) ? null : new Regex(Config.Current.Bookshelf.ExcludePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FolderList exclude: {ex.Message}");
                ExcludeRegex = null;
            }
        }

        protected override void OnPlaceChanged(object? sender, FolderSetPlaceOption options)
        {
            if (_disposedValue) return;

            base.OnPlaceChanged(sender, options);

            if (options.HasFlag(FolderSetPlaceOption.UpdateHistory))
            {
                if (Place is null) return;
                var place = Place with { Search = null };
                this.History?.Add(place);
            }
        }

        #region FolderHistory

        public List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return History.GetHistory(direction, size);
        }

        public List<KeyValuePair<int, QueryPath>> GetPreviousHistory()
        {
            return GetHistory(-1, 10);
        }

        public List<KeyValuePair<int, QueryPath>> GetNextHistory()
        {
            return GetHistory(+1, 10);
        }

        public bool CanMoveToPrevious()
        {
            if (_disposedValue) return false;

            return this.History.CanMoveToPrevious();
        }

        public override async void MoveToPrevious()
        {
            if (_disposedValue) return;

            await this.History.MoveToPreviousAsync();
        }

        public bool CanMoveToNext()
        {
            if (_disposedValue) return false;

            return this.History.CanMoveToNext();
        }

        public override async void MoveToNext()
        {
            if (_disposedValue) return;

            await this.History.MoveToNextAsync();
        }

        public async void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            if (_disposedValue) return;

            await this.History.MoveToHistoryAsync(item);
        }

        // NOTE: Historyから呼ばれる
        public async ValueTask MoveToHistoryAsync(QueryPath path)
        {
            if (_disposedValue) return;

            await SetPlaceAsync(path, null, FolderSetPlaceOption.Focus);
            CloseBookIfNecessary();
        }

        #endregion FolderHistory

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
        public class BookshelfSearchBoxComponent : ISearchBoxComponent
        {
            private readonly BookshelfFolderList _self;

            public BookshelfSearchBoxComponent(BookshelfFolderList self)
            {
                _self = self;
            }

            public HistoryStringCollection? History => BookHistoryCollection.Current.BookshelfSearchHistory;

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.RequestSearchPlace(keyword, false);
        }

    }

}
