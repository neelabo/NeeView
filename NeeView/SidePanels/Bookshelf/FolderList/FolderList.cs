using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeView.Collections.Generic;
using NeeLaboratory.Linq;
using NeeView.Windows.Controls;
using NeeView.Windows.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NeeLaboratory.Generators;
using NeeLaboratory.Threading;
using NeeLaboratory.Threading.Tasks;

namespace NeeView
{
    // TODO: 非同期処理はキャンセルできるうように
    // TODO: FolderCollectionFactory に SearchEngine を渡しているのがよろしくない

    /// <summary>
    /// FolderList Model
    /// </summary>
    public abstract partial class FolderList : BindableBase, IDisposable
    {
        #region IsMoving (static)

        private static readonly ReferenceCounter _isMovingCount = new();

        /// <summary>
        /// フォルダー移動処理中イベント.
        /// </summary>
        [Subscribable]
        public static event EventHandler<ReferenceCounterChangedEventArgs> IsMovingChanged
        {
            add { _isMovingCount.Changed += value; }
            remove { _isMovingCount.Changed -= value; }
        }

        /// <summary>
        /// フォルダー移動処理中？
        /// </summary>
        /// <remarks>
        /// 本の切り替え処理中であるかの判定に利用
        /// </remarks>
        public static bool IsMoving => _isMovingCount.IsActive;

        #endregion

        /// <summary>
        /// そのフォルダーで最後に選択されていた項目の記憶
        /// </summary>
        private readonly Dictionary<QueryPath, FolderItemPosition> _lastPlaceDictionary = new();

        /// <summary>
        /// 更新フラグ
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// 検索エンジン
        /// </summary>
        private readonly FileSearchEngineProxy _searchEngine;

        // フォルダーコレクション生成
        private readonly FolderCollectionFactory _folderCollectionFactory;

        private CancellationTokenSource _updateFolderCancellationTokenSource = new();
        private CancellationTokenSource _cruiseFolderCancellationTokenSource = new();

        private readonly FolderListConfig _folderListConfig;

        private volatile bool _isCollectionCreating;
        private List<Action> _collectionCreatedCallback = new();

        private ReferenceCounter _busyCount = new();
        private BusyLockEvent _busyLockEvent = new();

        private FolderCollection? _folderCollection;
        private FolderItem? _selectedItem;

        private readonly System.Threading.Lock _lock = new();

        private double _areaWidth = double.PositiveInfinity;
        private double _areaHeight = double.PositiveInfinity;
        private bool _isFocusAtOnce;

        private readonly DisposableCollection _disposables = new();

        private SearchBoxModel? _searchBoxModel;

        protected FolderList(bool isSyncBookHub, bool isOverlayEnabled, FolderListConfig folderListConfig)
        {
            _folderListConfig = folderListConfig;

            _searchEngine = new FileSearchEngineProxy();

            _folderCollectionFactory = new FolderCollectionFactory(_searchEngine, isOverlayEnabled);

            _disposables.Add(_busyLockEvent);

            if (isSyncBookHub)
            {
                _disposables.Add(BookHub.Current.SubscribeFolderListSync(
                    (s, e) => AppDispatcher.Invoke(() => SyncWeak(e))));

                _disposables.Add(BookHistoryCollection.Current.SubscribeHistoryChanged(
                    BookHistoryCollection_HistoryChanged));

                //e.NewItems.ForEach(item => RefreshIcon(new QueryPath(item.Path)))));

                _disposables.Add(BookHub.Current.SubscribeLoadRequested(
                    (s, e) => CancelMoveCruiseFolder()));
            }

            if (isOverlayEnabled)
            {
                _disposables.Add(BookHub.Current.SubscribeBookmarkChanged((s, e) =>
                {
                    if (_disposedValue) return;

                    switch (e.Action)
                    {
                        case EntryCollectionChangedAction.Reset:
                        case EntryCollectionChangedAction.Replace:
                            RefreshIcon(null);
                            break;
                        case EntryCollectionChangedAction.Update:
                            break;
                        default:
                            if (e.Item?.Value is Bookmark bookmark)
                            {
                                RefreshIcon(new QueryPath(bookmark.Path));
                            }
                            break;
                    }
                }));
            }

            // ブックマーク監視
            _disposables.Add(BookmarkCollection.Current.SubscribeBookmarkChanged(BookmarkCollection_BookmarkChanged));

            _disposables.Add(_folderListConfig.SubscribePropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) =>
            {
                RaisePropertyChanged(nameof(PanelListItemStyle));
            }));

            _disposables.Add(_folderListConfig.SubscribePropertyChanged(nameof(FolderListConfig.IsFolderTreeVisible), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsFolderTreeVisible));
                RaisePropertyChanged(nameof(FolderTreeAreaWidth));
                RaisePropertyChanged(nameof(FolderTreeAreaHeight));
            }));

            _disposables.Add(_folderListConfig.SubscribePropertyChanged(nameof(FolderListConfig.FolderTreeLayout), (s, e) =>
            {
                RaisePropertyChanged(nameof(FolderTreeLayout));
                RaisePropertyChanged(nameof(FolderTreeAreaWidth));
                RaisePropertyChanged(nameof(FolderTreeAreaHeight));
            }));
        }


        // 場所変更
        [Subscribable]
        public event EventHandler? PlaceChanged;

        // FolderCollection総入れ替え
        [Subscribable]
        public event EventHandler? CollectionChanged;

        // 検索ボックスにフォーカスを
        [Subscribable]
        public event EventHandler? SearchBoxFocus;

        // フォルダーツリーにフォーカスを
        [Subscribable]
        public event EventHandler? FolderTreeFocus;

        // リスト更新処理中イベント
        [Subscribable]
        public event EventHandler<ReferenceCounterChangedEventArgs>? BusyChanged
        {
            add => _busyCount.Changed += value;
            remove => _busyCount.Changed -= value;
        }

        // 選択変更開始
        [Subscribable]
        public event EventHandler<FolderListSelectedChangedEventArgs>? SelectedChanging;

        // 選択変更
        [Subscribable]
        public event EventHandler<FolderListSelectedChangedEventArgs>? SelectedChanged;


        /// <summary>
        /// 選択項目
        /// </summary>
        public FolderItem? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// リスト自体のコンテキストメニュー表示が有効？
        /// </summary>
        public bool IsContextMenuEnabled => FolderCollection is BookmarkFolderCollection;

        /// <summary>
        /// フォーカス要求
        /// </summary>
        public bool IsFocusAtOnce
        {
            get { return _isFocusAtOnce; }
            set { SetProperty(ref _isFocusAtOnce, value); }
        }

        /// <summary>
        /// 本を読み込むときに本棚の更新を要求する
        /// </summary>
        public virtual bool IsSyncBookshelfEnabled
        {
            get { return false; }
            set { }
        }

        public FolderListConfig FolderListConfig => _folderListConfig;


        public PanelListItemStyle PanelListItemStyle
        {
            get { return _folderListConfig.PanelListItemStyle; }
            set { _folderListConfig.PanelListItemStyle = value; }
        }

        // サムネイル画像が表示される？？
        public bool IsThumbnailVisible
        {
            get
            {
                return _folderListConfig.PanelListItemStyle switch
                {
                    PanelListItemStyle.Thumbnail => true,
                    PanelListItemStyle.Content => Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0,
                    PanelListItemStyle.Banner => Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0,
                    _ => false,
                };
            }
        }

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        public FolderCollection? FolderCollection
        {
            get { return _folderCollection; }
            private set
            {
                if (_folderCollection != value)
                {
                    _folderCollection?.Dispose();
                    _folderCollection = value;
                }
            }
        }

        /// <summary>
        /// 検索ボックス
        /// </summary>
        public SearchBoxModel? SearchBoxModel
        {
            get { return _searchBoxModel; }
            protected set { SetProperty(ref _searchBoxModel, value); }
        }


        /// <summary>
        /// 検索許可？
        /// </summary>
        public bool IsFolderSearchEnabled => FolderCollection != null && FolderCollection.IsSearchEnabled;


        /// <summary>
        /// 現在のフォルダー
        /// </summary>
        public QueryPath? Place => _folderCollection?.Place;

        /// <summary>
        /// 現在のフォルダーが有効？
        /// </summary>
        public bool IsPlaceValid => Place != null;


        public bool IsFolderTreeVisible
        {
            get => _folderListConfig.IsFolderTreeVisible;
            set => _folderListConfig.IsFolderTreeVisible = value;
        }

        public FolderTreeLayout FolderTreeLayout
        {
            get => FolderListConfig.FolderTreeLayout;
            set => FolderListConfig.FolderTreeLayout = value;
        }

        /// <summary>
        /// フォルダーツリーエリアの幅
        /// </summary>
        public double FolderTreeAreaWidth
        {
            get
            {
                if (this.IsFolderTreeVisible && this.FolderTreeLayout == FolderTreeLayout.Left)
                {
                    return _folderListConfig.FolderTreeAreaWidth;
                }
                else
                {
                    return 0.0;
                }
            }
            set
            {
                if (this.IsFolderTreeVisible && this.FolderTreeLayout == FolderTreeLayout.Left)
                {
                    var width = Math.Max(Math.Min(value, _areaWidth - 32.0), 32.0 - 6.0);
                    if (_folderListConfig.FolderTreeAreaWidth != width)
                    {
                        _folderListConfig.FolderTreeAreaWidth = width;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// フォルダーリストエリアの幅
        /// クイックアクセスエリアの幅計算用
        /// </summary>
        public double AreaWidth
        {
            get { return _areaWidth; }
            set
            {
                if (SetProperty(ref _areaWidth, value))
                {
                    // 再設定する
                    FolderTreeAreaWidth = _folderListConfig.FolderTreeAreaWidth;
                }
            }
        }

        /// <summary>
        /// フォルダーツリーエリアの高さ
        /// </summary>
        public double FolderTreeAreaHeight
        {
            get
            {
                if (this.IsFolderTreeVisible && this.FolderTreeLayout == FolderTreeLayout.Top)
                {
                    return _folderListConfig.FolderTreeAreaHeight;
                }
                else
                {
                    return 0.0;
                }
            }
            set
            {
                if (this.IsFolderTreeVisible && this.FolderTreeLayout == FolderTreeLayout.Top)
                {
                    var height = Math.Max(Math.Min(value, _areaHeight - 32.0), 32.0 - 6.0);
                    if (_folderListConfig.FolderTreeAreaHeight != height)
                    {
                        _folderListConfig.FolderTreeAreaHeight = height;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// フォルダーリストエリアの高さ
        /// クイックアクセスエリアの高さ計算用
        /// </summary>
        public double AreaHeight
        {
            get { return _areaHeight; }
            set
            {
                if (SetProperty(ref _areaHeight, value))
                {
                    // 再設定する
                    FolderTreeAreaHeight = _folderListConfig.FolderTreeAreaHeight;
                }
            }
        }

        // 現在の場所のフォルダーの並び順
        public FolderOrder FolderOrder
        {
            get { return GetFolderOrder(); }
        }

        public bool IsFolderOrderEnabled
        {
            get
            {
                if (_folderCollection is null)
                {
                    return false;
                }
                if (_folderCollection is FolderEntryCollection collection)
                {
                    return collection.Place.Path != null;
                }
                else
                {
                    return _folderCollection.FolderOrderClass != FolderOrderClass.None;
                }
            }
        }

        /// <summary>
        /// サブフォルダーを検索範囲に含める
        /// </summary>
        public virtual bool IsSearchIncludeSubdirectories
        {
            get { return false; }
            set { }
        }


        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            switch (e.HistoryChangedType)
            {
                case BookMementoCollectionChangedType.Reset:
                case BookMementoCollectionChangedType.Load:
                    RefreshIcon(null);
                    break;
                default:
                    foreach (var item in e.NewItems.Union(e.OldItems))
                    {
                        RefreshIcon(new QueryPath(item.Path));
                    }
                    break;
            }
        }

        private void RaiseCollectionChanged()
        {
            if (_disposedValue) return;

            CollectionChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(FolderCollection));
            RaisePropertyChanged(nameof(Place));
            RaisePropertyChanged(nameof(IsPlaceValid));
            RaisePropertyChanged(nameof(FolderOrder));
            RaisePropertyChanged(nameof(IsFolderOrderEnabled));
            RaisePropertyChanged(nameof(IsFolderSearchEnabled));
        }

        public virtual void IsVisibleChanged(bool isVisible)
        {
        }

        /// <summary>
        /// フォーカス要求
        /// </summary>
        public void FocusAtOnce()
        {
            if (_disposedValue) return;

            this.IsFocusAtOnce = true;
        }

        /// <summary>
        /// HOME取得
        /// </summary>
        public abstract QueryPath GetFixedHome();


        /// <summary>
        /// フォルダー状態保存
        /// </summary>
        private void SavePlace(QueryPath? place, FolderItem? folder, int index)
        {
            if (folder == null || place == null) return;
            Debug.Assert(folder.Place == place);

            _lastPlaceDictionary[place] = new FolderItemPosition(folder.TargetPath, index);
        }


        #region Search

        /// <summary>
        /// 検索キーワードチェック
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public SearchKeywordAnalyzeResult SearchKeywordAnalyze(string keyword)
        {
            try
            {
                return new SearchKeywordAnalyzeResult(_searchEngine.Analyze(keyword));
            }
            catch (Exception ex)
            {
                return new SearchKeywordAnalyzeResult(ex);
            }
        }

        /// <summary>
        /// 検索更新
        /// </summary>
        public void RequestSearchPlace(bool isForce)
        {
            if (_searchBoxModel is null) return;
            RequestSearchPlace(_searchBoxModel.FixedKeyword, isForce);
        }

        protected void RequestSearchPlace(string keyword, bool isForce)
        {
            if (_disposedValue) return;
            if (_searchBoxModel is null) return;
            if (Place is null || !IsFolderSearchEnabled) return;

            // 文法エラーがあるならば更新しない
            if (_searchBoxModel.KeywordErrorMessage != null)
            {
                return;
            }

            var query = Place with { Search = keyword };
            var option = isForce ? FolderSetPlaceOption.Refresh : FolderSetPlaceOption.None;
            RequestPlace(query, null, option);
        }

        #endregion Search

        /// <summary>
        /// フォルダーリスト安定チェック
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async ValueTask WaitAsync(CancellationToken cancellationToken)
        {
            await _busyLockEvent.WaitHandle.AsTask().WaitAsync(cancellationToken);
        }

        /// <summary>
        /// フォルダーリスト更新要求
        /// </summary>
        public void RequestPlace(QueryPath path, FolderItemPosition? select, FolderSetPlaceOption options)
        {
            if (_disposedValue) return;

            if (!CheckScheme(path))
            {
                return;
            }

            _ = SetPlaceAsync(path, select, options);
        }

        /// <summary>
        /// パスがサポートしているスキームであるか判定
        /// </summary>
        protected virtual bool CheckScheme(QueryPath query)
        {
            return true;
        }


        public void SetDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="place">フォルダーパス</param>
        /// <param name="select">初期選択項目</param>
        public async ValueTask SetPlaceAsync(QueryPath path, FolderItemPosition? select, FolderSetPlaceOption options)
        {
            if (_disposedValue) return;

            if (path == null)
            {
                return;
            }

            path = path.ResolvePath();

            using var busyLock = _busyLockEvent.CreateBusyLock();

            // 現在フォルダーの情報を記憶
            SavePlace(Place, SelectedItem, GetFolderItemIndex(SelectedItem));

            // 初期項目
            if (select == null)
            {
                _lastPlaceDictionary.TryGetValue(path, out select);
            }

            if (options.HasFlag(FolderSetPlaceOption.TopSelect))
            {
                select = null;
            }

            // コレクション生成中ならばキャンセル
            _updateFolderCancellationTokenSource?.Cancel();
            _updateFolderCancellationTokenSource?.Dispose();
            _updateFolderCancellationTokenSource = new CancellationTokenSource();
            var token = _updateFolderCancellationTokenSource.Token;

            // 更新が必要であれば、新しいFolderListBoxを作成する
            if (CheckFolderListUpdateIfNecessary(path, options))
            {
                try
                {
                    OnCollectionCreating();
                    _isDirty = false;

                    // FolderCollection 更新
                    var collection = await CreateFixedFolderCollectionAsync(path, true, token);
                    if (collection != null)
                    {
                        this.FolderCollection = collection;
                        this.FolderCollection.CollectionChanging += FolderCollection_CollectionChanging;
                        this.FolderCollection.CollectionChanged += FolderCollection_CollectionChanged;
                        RaiseCollectionChanged();

                        if (Place is null) throw new InvalidOperationException("Place is not null when FolderCollection is not null");

                        SetSelectedItem(FixedItemPrioritizeCurrentBook(select), options.HasFlag(FolderSetPlaceOption.Focus));

                        // 最終フォルダー更新
                        Config.Current.StartUp.LastFolderPath = Place.SimpleQuery;

                        // 検索キーワード更新
                        _searchBoxModel?.ResetInputKeyword(Place.Search);

                        OnPlaceChanged(this, options);
                        PlaceChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                finally
                {
                    OnCollectionCreated();
                }
            }
            else
            {
                // 選択項目のみ変更
                SetSelectedItem(FixedItem(select), false);
                PlaceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// PlaceChanged
        /// </summary>
        protected virtual void OnPlaceChanged(object? sender, FolderSetPlaceOption options)
        {
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        private bool CheckFolderListUpdateIfNecessary(QueryPath path, FolderSetPlaceOption options)
        {
            if (_isDirty || _folderCollection == null || path != _folderCollection.Place || options.HasFlag(FolderSetPlaceOption.Refresh))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        public async ValueTask RefreshAsync(bool force, bool resetSearchEngine)
        {
            if (_disposedValue) return;

            if (_folderCollection == null) return;
            if (Place is null) throw new InvalidOperationException("Place is not null when FolderCollection is not null");

            _isDirty = force || _folderCollection.IsDirty();

            if (resetSearchEngine)
            {
                _searchEngine.Reset();
            }

            await SetPlaceAsync(Place, null, FolderSetPlaceOption.UpdateHistory);
        }

        /// <summary>
        /// 現在開いているフォルダーで更新(弱)
        /// e.isKeepPlaceが有効の場合、すでにリストに存在している場合は何もしない
        /// </summary>
        public async ValueTask SyncWeak(FolderListSyncEventArgs e)
        {
            if (_disposedValue) return;

#if false
            if (IsLocked)
            {
                return;
            }
#endif

            // TODO: 
            var parent = new QueryPath(e.Parent);
            var path = new QueryPath(e.Path);

            var collection = _folderCollection;

            if (e != null && e.IsKeepPlace)
            {
                // すでにリストに存在している場合は何もしない
                if (collection == null || collection.Contains(path)) return;
            }

            var options = FolderSetPlaceOption.UpdateHistory;

            if (collection != null)
            {
                if (collection.Place.FullPath == parent.FullPath && collection.Contains(path))
                {
                    await SetPlaceAsync(collection.Place, new FolderItemPosition(path), options);
                    return;
                }
            }

            await SetPlaceAsync(parent, new FolderItemPosition(path), options);
        }


        /// <summary>
        /// 検索ボックスにフォーカス要求
        /// </summary>
        public void RaiseSearchBoxFocus()
        {
            if (_disposedValue) return;

            SearchBoxFocus?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// フォルダーツリーにフォーカス要求
        /// </summary>
        public void RaiseFolderTreeFocus()
        {
            if (_disposedValue) return;

            FolderTreeFocus?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 現在の場所の履歴を削除
        /// </summary>
        public void ClearHistory()
        {
            if (_disposedValue) return;

            var items = _folderCollection?.Items.Select(e => e.TargetPath.SimplePath).Where(e => e != null);
            if (items is not null)
            {
                BookHistoryCollection.Current.Remove(items);
            }

            RefreshIcon(null);
        }

        #region MoveFolder

        // 次のフォルダーに移動
        public async ValueTask NextFolder(bool isPrioritizeBookMove, BookLoadOption option = BookLoadOption.None)
        {
            if (_disposedValue) return;

            if (!isPrioritizeBookMove && BookHub.Current.IsBusy) return;

            try
            {
                _isMovingCount.Increment();
                var result = await MoveFolder(+1, option);
                if (result != true)
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.TextResources.GetString("Notice.BookNextFailed"));
                }
            }
            finally
            {
                _isMovingCount.Decrement();
            }
        }

        // 前のフォルダーに移動
        public async ValueTask PrevFolder(bool isPrioritizeBookMove, BookLoadOption option = BookLoadOption.None)
        {
            if (_disposedValue) return;

            if (!isPrioritizeBookMove && BookHub.Current.IsBusy) return;

            try
            {
                _isMovingCount.Increment();
                var result = await MoveFolder(-1, option);
                if (result != true)
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.TextResources.GetString("Notice.BookPrevFailed"));
                }
            }
            finally
            {
                _isMovingCount.Decrement();
            }
        }

        // ランダムなフォルダーに移動
        public async ValueTask RandomFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_disposedValue) return;

            if (BookHub.Current.IsBusy) return;

            try
            {
                _isMovingCount.Increment();
                await MoveRandomFolder(option);
            }
            finally
            {
                _isMovingCount.Decrement();
            }
        }

        // 巡回移動できる？
        protected virtual bool IsCruise()
        {
            return false;
        }

        /// <summary>
        /// コマンドの「前のフォルダーに移動」「次のフォルダーへ移動」に対応
        /// </summary>
        private async ValueTask<bool> MoveFolder(int direction, BookLoadOption options)
        {
            var isCruise = IsCruise() && _folderCollection is not FolderSearchCollection;
            if (isCruise)
            {
                return await MoveCruiseFolder(direction, options);
            }
            else
            {
                return await MoveNextFolder(direction, options);
            }
        }

        /// <summary>
        /// 通常フォルダー移動
        /// </summary>
        private async ValueTask<bool> MoveNextFolder(int direction, BookLoadOption options)
        {
            if (_folderCollection is null) return false;

            var item = GetFolderItem(SelectedItem, direction);
            if (item == null) return false;

            int index = GetFolderItemIndex(item);

            await SetPlaceAsync(_folderCollection.Place, new FolderItemPosition(item.TargetPath, index), FolderSetPlaceOption.UpdateHistory);
            RequestLoad(item, null, options, false);

            return true;
        }

        /// <summary>
        /// ランダムフォルダー移動
        /// </summary>
        private async ValueTask<bool> MoveRandomFolder(BookLoadOption options)
        {
            if (_folderCollection is null) return false;

            var currentBookAddress = BookOperation.Current.Book?.Path;

            var items = _folderCollection.Where(e => !e.IsEmpty() && e.EntityPath.Scheme == QueryScheme.File && e.EntityPath.SimplePath != currentBookAddress);
            if (!items.Any())
            {
                return false;
            }

            var item = items.ElementAt(new Random().Next(items.Count()));
            if (item == null)
            {
                return false;
            }

            int index = GetFolderItemIndex(item);

            await SetPlaceAsync(_folderCollection.Place, new FolderItemPosition(item.TargetPath, index), FolderSetPlaceOption.UpdateHistory);
            RequestLoad(item, null, options, false);

            return true;
        }

        /// <summary>
        /// 巡回フォルダー移動
        /// </summary>
        private async ValueTask<bool> MoveCruiseFolder(int direction, BookLoadOption options)
        {
            // TODO: NowLoad表示をどうしよう。BookHubに処理を移してそこで行う？

            if (_folderCollection is null) return false;

            var item = SelectedItem;
            if (item == null) return false;

            _cruiseFolderCancellationTokenSource?.Cancel();
            _cruiseFolderCancellationTokenSource?.Dispose();
            _cruiseFolderCancellationTokenSource = new CancellationTokenSource();
            var token = _cruiseFolderCancellationTokenSource.Token;

            try
            {
                var node = new FolderNode(_folderCollection, item);
                var next = (direction < 0) ? await node.CruisePrev(token) : await node.CruiseNext(token);
                if (next == null) return false;
                if (next.Content == null) return false;

                await SetPlaceAsync(new QueryPath(next.Place), new FolderItemPosition(next.Content.TargetPath), FolderSetPlaceOption.UpdateHistory);
                RequestLoad(next.Content, null, options, false);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cruise Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 巡回フォルダー移動キャンセル
        /// </summary>
        public void CancelMoveCruiseFolder()
        {
            _cruiseFolderCancellationTokenSource?.Cancel();
        }

        private void RequestLoad(FolderItem item, string? start, BookLoadOption option, bool isRefreshFolderList)
        {
            if (_disposedValue) return;

            var defaultRecursiveOptionFlag = IsFolderRecursive(item.Place) ? BookLoadOption.DefaultRecursive : BookLoadOption.None;
            var undeletableOptionFlag = item.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeletable;
            var options = option | BookLoadOption.IsBook | defaultRecursiveOptionFlag | undeletableOptionFlag;
            BookHub.Current.RequestLoad(this, item.TargetPath.SimplePath, start, options, isRefreshFolderList);
        }

        /// <summary>
        /// 再帰フォルダーが既定の場所であるか
        /// </summary>
        private static bool IsFolderRecursive(QueryPath? path)
        {
            if (path is null) return false;

            var memento = BookHistoryCollection.Current.GetFolderMemento(path.SimplePath);
            return memento.IsFolderRecursive;
        }

        #endregion MoveFolder

        #region CreateFolderCollection

        /// <summary>
        /// コレクション作成
        /// </summary>
        public async ValueTask<FolderCollection?> CreateFixedFolderCollectionAsync(QueryPath path, bool isForce, CancellationToken token)
        {
            if (_disposedValue) return null;

            try
            {
                _busyCount.Increment();

                var collection = await CreateFolderCollectionAsync(path, isForce, token);
                if (collection != null && !token.IsCancellationRequested)
                {
                    collection.ParameterChanged += async (s, e) => await RefreshAsync(true, false);
                    return collection;
                }
                else
                {
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"UpdateFolderCollectionAsync: Canceled: {path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateFolderCollectionAsync: {ex.Message}");
            }
            finally
            {
                _busyCount.Decrement();
            }

            return null;
        }

        /// <summary>
        /// コレクション作成
        /// </summary>
        private async ValueTask<FolderCollection?> CreateFolderCollectionAsync(QueryPath path, bool isForce, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (!isForce && _folderCollection != null && _folderCollection.Place.Equals(path))
            {
                return null;
            }

            // 場所が変更された場合は検索初期化
            _searchEngine.ResetIfConditionChanged(path.SimplePath);

            return await _folderCollectionFactory.CreateFolderCollectionAsync(path, true, IsSearchIncludeSubdirectories, token);
        }

        #endregion CreateFolderCollection

        #region Commands
        // NOTE: RelayCommandの実体なので、async void が使用されている場合がある。

        public void AddQuickAccess()
        {
            if (_disposedValue) return;

            _folderListConfig.IsFolderTreeVisible = true;
            var query = GetCurrentQueryPath();
            if (query is null) return;
            FolderPanel.Current.FolderTreeModel?.AddQuickAccess(query);
        }

        public string? GetCurrentQueryPath()
        {
            return Place?.SimpleQuery;
        }

        // TODO: BookshelfFolderList じゃね？
        public void ClearHistoryInPlace()
        {
            if (_disposedValue) return;

            // TODO: コマンドKeyはクラス名から生成するように
            var command = CommandTable.Current.GetElement("ClearHistoryInPlace");
            if (command.CanExecute(this, CommandArgs.Empty))
            {
                command.Execute(this, CommandArgs.Empty);
            }
        }

        public bool CanSetHome()
        {
            if (_disposedValue) return false;

            return Place != null;
        }

        public void SetHome()
        {
            if (_disposedValue) return;

            if (BookHub.Current == null) return;
            if (Place == null) return;
            Config.Current.Bookshelf.Home = Place.SimpleQuery;
        }

        public async void MoveToHome()
        {
            if (_disposedValue) return;

            if (BookHub.Current == null) return;

            var place = GetFixedHome();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.TopSelect | FolderSetPlaceOption.ResetKeyword);

            CloseBookIfNecessary();
        }

        public async void MoveTo(QueryPath? path)
        {
            if (_disposedValue) return;

            if (path is null) return;

            await this.SetPlaceAsync(path, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);

            CloseBookIfNecessary();
        }


        public virtual void MoveToPrevious()
        {
        }

        public virtual void MoveToNext()
        {
        }

        public virtual bool CanMoveToParent()
        {
            if (_disposedValue) return false;

            var parentQuery = _folderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return true;
        }

        public async void MoveToParent()
        {
            if (_disposedValue) return;

            if (!CanMoveToParent()) return;

            if (Place is null) return;

            var parent = _folderCollection?.GetParentQuery();
            if (parent == null) return;


            await SetPlaceAsync(parent, new FolderItemPosition(Place), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory);
            CloseBookIfNecessary();
        }

        public abstract void Sync();


        public void ToggleFolderRecursive()
        {
            if (_disposedValue) return;

            ToggleFolderRecursive_Executed();
        }

        protected virtual void CloseBookIfNecessary()
        {
        }

        #region FolderCollection生成衝突の回避用

        private void AddCollectionCreatedCallback(Action callback)
        {
            lock (_lock)
            {
                _collectionCreatedCallback.Add(callback);
            }
        }

        private void OnCollectionCreating()
        {
            lock (_lock)
            {
                _isCollectionCreating = true;
            }
        }

        private void OnCollectionCreated()
        {
            List<Action> collections;

            lock (_lock)
            {
                _isCollectionCreating = false;
                if (_collectionCreatedCallback.Count == 0) return;
                collections = _collectionCreatedCallback;
                _collectionCreatedCallback = new List<Action>();
            }

            foreach (var callback in collections)
            {
                callback.Invoke();
            }
        }

        #endregion

        /// <summary>
        /// ブックマークの変更監視
        /// </summary>
        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (_isCollectionCreating)
            {
                AddCollectionCreatedCallback(() => BookmarkCollection_BookmarkChanged(sender, e));
                return;
            }

            if (FolderCollection is not BookmarkFolderCollection folderCollection)
            {
                return;
            }

            switch (e.Action)
            {
                case EntryCollectionChangedAction.Remove:
                    if (!BookmarkCollection.Current.Contains(folderCollection.BookmarkPlace))
                    {
                        RefreshBookmarkFolder();
                    }
                    break;

                case EntryCollectionChangedAction.Rename:
                    if (!BookmarkCollection.Current.Contains(folderCollection.BookmarkPlace))
                    {
                        RefreshBookmarkFolder();
                    }
                    else
                    {
                        var query = folderCollection.BookmarkPlace.CreateQuery();
                        if (!folderCollection.Place.Equals(query))
                        {
                            RequestPlace(query, null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Replace:
                case EntryCollectionChangedAction.Reset:
                    RefreshBookmarkFolder();
                    break;
            }
        }

        /// <summary>
        /// ブックマークフォルダーを同じパスで作り直す。存在しなければルートで作る。
        /// </summary>
        private void RefreshBookmarkFolder()
        {
            if (_disposedValue) return;

            if (FolderCollection is not BookmarkFolderCollection)
            {
                return;
            }

            ////Debug.WriteLine($"{this}: Refresh BookmarkFolder");
            var query = FolderCollection.Place;
            var node = BookmarkCollection.Current.FindNode(query);
            if (node == null || node.Value is not BookmarkFolder)
            {
                query = new QueryPath(QueryScheme.Bookmark, null, null);
            }

            RequestPlace(query, null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
        }

        #endregion Commands

        internal void SetSelectedItem(FolderItem? item, bool isFocus)
        {
            if (_disposedValue) return;

            RaiseSelectedItemChanging();
            this.SelectedItem = item;
            RaiseSelectedItemChanged(isFocus);
        }


        /// <summary>
        /// ふさわしい選択項目インデックスを取得
        /// </summary>
        /// <param name="path">選択したいパス</param>
        /// <returns></returns>
        internal int FixedIndexOfPath(QueryPath path)
        {
            if (_disposedValue) return 0;

            if (_folderCollection is null) return 0;

            var index = _folderCollection.IndexOfPath(path);
            return index < 0 ? 0 : index;
        }

        /// <summary>
        /// 選択項目の復元 (現在ブックを優先)
        /// </summary>
        internal FolderItem? FixedItemPrioritizeCurrentBook(FolderItemPosition? pos)
        {
            if (_disposedValue) return null;

            var item = FindFolderItem(BookHub.Current.GetCurrentBook()?.SourcePath);
            return item ?? FixedItem(pos);
        }

        /// <summary>
        /// 選択項目の復元
        /// </summary>
        internal FolderItem? FixedItem(FolderItemPosition? pos)
        {
            if (_disposedValue) return null;

            if (_folderCollection is null) return null;

            if (pos == null)
            {
                return _folderCollection.FirstOrDefault();
            }

            if (pos.Index >= 0)
            {
                var item = _folderCollection.Items.ElementAtOrDefault(pos.Index);
                if (item != null && item.TargetPath == pos.Path)
                {
                    return item;
                }
            }

            // アーカイブ内のパスの場合、有効な項目になるまで場所を遡る
            var path = pos.Path;
            do
            {
                var select = _folderCollection.Items.FirstOrDefault(e => e.TargetPath == path);
                if (select != null)
                {
                    return select;
                }
                path = path.GetParent();
            }
            while (path != null && path.FullPath.Length > _folderCollection.Place.FullPath.Length);
            return _folderCollection.FirstOrDefault();
        }

        /// <summary>
        /// 項目変更前通知
        /// </summary>
        public void RaiseSelectedItemChanging()
        {
            if (_disposedValue) return;

            SelectedChanging?.Invoke(this, new FolderListSelectedChangedEventArgs());
        }

        /// <summary>
        /// 項目変更後通知
        /// </summary>
        /// <param name="isFocus"></param>
        public void RaiseSelectedItemChanged(bool isFocus = false)
        {
            if (_disposedValue) return;

            SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = isFocus });
        }


        // となりを取得
        public FolderItem? GetNeighbor(FolderItem item)
        {
            if (_disposedValue) return null;

            var items = this.FolderCollection?.Items;
            if (items == null || items.Count <= 0) return null;

            int index = items.IndexOf(item);
            if (index < 0) return items[0];

            if (index + 1 < items.Count)
            {
                return items[index + 1];
            }
            else if (index > 0)
            {
                return items[index - 1];
            }
            else
            {
                return item;
            }
        }

        private void FolderCollection_CollectionChanging(object? sender, FolderCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (e.Action == CollectionChangeAction.Remove)
            {
                SelectedChanging?.Invoke(this, new FolderListSelectedChangedEventArgs());
                if (SelectedItem == e.Item)
                {
                    SelectedItem = GetNeighbor(SelectedItem);
                }
            }
        }

        private void FolderCollection_CollectionChanged(object? sender, FolderCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (e.Action == CollectionChangeAction.Remove)
            {
                if (SelectedItem == null)
                {
                    SelectedItem = _folderCollection?.Items?.FirstOrDefault();
                }
                SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs());
            }
        }

        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem? GetFolderItem(FolderItem? item, int offset)
        {
            if (_disposedValue) return null;

            if (item is null) return null;
            if (this.FolderCollection?.Items == null) return null;

            int index = this.FolderCollection.Items.IndexOf(item);
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
        }

        internal int GetFolderItemIndex(FolderItem? item)
        {
            if (_disposedValue) return -1;

            if (item is null) return -1;
            if (this.FolderCollection?.Items == null) return -1;

            return this.FolderCollection.Items.IndexOf(item);
        }


        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefreshIcon(QueryPath? path)
        {
            if (_disposedValue) return;

            this.FolderCollection?.RefreshIcon(path);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item)
        {
            LoadBook(item, ArchiveHint.None);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item, ArchiveHint archiveHint)
        {
            if (_disposedValue) return;

            if (item == null) return;
            if (_folderCollection is null) return;

            BookLoadOption option = BookLoadOption.SkipSamePlace;
            if (_folderCollection.FolderParameter.IsFolderRecursive)
            {
                option |= BookLoadOption.DefaultRecursive;
            }

            LoadBook(item, option, archiveHint);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item, BookLoadOption option, ArchiveHint archiveHint)
        {
            if (_disposedValue) return;

            if (item.Attributes.HasFlag(FolderItemAttribute.System))
            {
                return;
            }

            // ブックマークフォルダーは本として開けないようにする
            if (item.Attributes.HasFlag(FolderItemAttribute.Directory | FolderItemAttribute.Bookmark))
            {
                return;
            }

            var query = item.TargetPath;
            var additionalOption = BookLoadOption.IsBook | BookLoadOption.KeepArchiveHint | (item.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeletable);
            BookHub.Current.RequestLoad(this, query.SimplePath, null, option | additionalOption, IsSyncBookshelfEnabled, archiveHint);
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            if (_disposedValue) return;

            if (FolderCollection == null) return;
            if (!FolderCollection.FolderOrderClass.GetFolderOrderMap().ContainsKey(folderOrder)) return;

            this.FolderCollection.FolderParameter.FolderOrder = folderOrder;
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// フォルダーの並びを取得
        /// </summary>
        public FolderOrder GetFolderOrder()
        {
            if (this.FolderCollection == null) return default;
            return this.FolderCollection.FolderParameter.FolderOrder;
        }

        /// <summary>
        /// フォルダーの並びを順番に切り替える
        /// </summary>
        public void ToggleFolderOrder()
        {
            if (_disposedValue) return;

            if (this.FolderCollection == null) return;
            SetFolderOrder(GetNextFolderOrder());
            RaisePropertyChanged(nameof(FolderOrder));
        }

        public FolderOrder GetNextFolderOrder()
        {
            if (this.FolderCollection == null) return default;

            var orders = FolderCollection.FolderOrderClass.GetFolderOrderMap().Keys;
            var now = this.FolderCollection.FolderParameter.FolderOrder;
            var index = orders.IndexOf(now);
            return orders.ElementAt((index + 1) % orders.Count);
        }

        public void ToggleFolderRecursive_Executed()
        {
            if (_disposedValue) return;

            if (_folderCollection is null) return;

            _folderCollection.FolderParameter.IsFolderRecursive = !_folderCollection.FolderParameter.IsFolderRecursive;
        }

        public void NewFolder()
        {
            if (_disposedValue) return;

            NewFolder(null);
        }

        public void NewFolder(string? name)
        {
            if (_disposedValue) return;

            if (FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                var nameless = string.IsNullOrWhiteSpace(name);

                var node = BookmarkCollection.Current.AddNewFolder(bookmarkFolderCollection.BookmarkPlace, name);
                if (node is null) return;

                var item = bookmarkFolderCollection.FirstOrDefault(e => e.Attributes.HasFlag(FolderItemAttribute.Directory) && e.Name == node.Value.Name);

                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = true, IsNewFolder = nameless });
                }
            }
        }

        public void SelectBookmark(TreeListNode<IBookmarkEntry> node, bool isFocus)
        {
            if (_disposedValue) return;

            if (FolderCollection is not BookmarkFolderCollection bookmarkFolderCollection)
            {
                return;
            }

            var item = bookmarkFolderCollection.FirstOrDefault(e => node == (e.Source as TreeListNode<IBookmarkEntry>));
            if (item != null)
            {
                SelectedItem = item;
                SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = isFocus });
            }
        }

        public bool AddBookmark()
        {
            if (_disposedValue) return false;

            var address = BookHub.Current.GetCurrentBook()?.Path;
            if (address == null)
            {
                return false;
            }

            return AddBookmark(new QueryPath(address), true);
        }

        public bool AddBookmark(QueryPath path, bool isFocus)
        {
            if (_disposedValue) return false;

            if (FolderCollection is not BookmarkFolderCollection bookmarkFolderCollection)
            {
                return false;
            }

            var node = BookmarkCollectionService.AddToChild(path, bookmarkFolderCollection.BookmarkPlace);
            if (node != null)
            {
                var item = bookmarkFolderCollection.FirstOrDefault(e => node == (e.Source as TreeListNode<IBookmarkEntry>));
                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new FolderListSelectedChangedEventArgs() { IsFocus = isFocus });
                }
            }

            return true;
        }

        public bool RemoveBookmark(IEnumerable<FolderItem> items)
        {
            if (_disposedValue) return false;

            var nodes = items.Select(e => e.Source as TreeListNode<IBookmarkEntry>).WhereNotNull().Reverse().ToList();
            if (!nodes.Any())
            {
                return false;
            }

            var mementos = new List<TreeListNodeMemento<IBookmarkEntry>>();
            int count = 0;

            foreach (var node in nodes)
            {
                var memento = new TreeListNodeMemento<IBookmarkEntry>(node);

                bool isRemoved = BookmarkCollection.Current.Remove(node);
                if (isRemoved)
                {
                    mementos.Add(memento);

                    if (node.Value is BookmarkFolder)
                    {
                        count += node.Count(e => e.Value is Bookmark);
                    }
                    else
                    {
                        count++;
                    }
                }
            }

            if (count >= 2)
            {
                var toast = new Toast(Properties.TextResources.GetFormatString("BookmarkFolderDelete.Message", count), null, ToastIcon.Information, Properties.TextResources.GetString("Word.Restore"),
                    () => { foreach (var memento in mementos) BookmarkCollection.Current.Restore(memento); });
                ToastService.Current.Show("BookmarkList", toast);
            }

            return (count > 0);
        }


        public FolderItem? FindFolderItem(string? address)
        {
            if (_disposedValue) return null;

            if (address is null) return null;
            if (_folderCollection is null) return null;

            var path = new QueryPath(address);
            var select = _folderCollection.Items.FirstOrDefault(e => e.TargetPath == path);

            return select;
        }

        public async ValueTask RemoveAsync(FolderItem item)
        {
            if (_disposedValue) return;

            await RemoveAsync(new FolderItem[] { item });
        }

        public async ValueTask RemoveAsync(IEnumerable<FolderItem> items)
        {
            if (_disposedValue) return;

            if (items == null) return;

            var bookmarks = items.Where(e => e.Attributes.HasFlag(FolderItemAttribute.Bookmark)).ToList();
            var files = items.Where(e => e.IsFileSystem()).ToList();

            if (bookmarks.Any())
            {
                RemoveBookmark(bookmarks);
            }
            else if (files.Any())
            {
                await RemoveFilesAsync(files);
            }
        }

        private async ValueTask RemoveFilesAsync(IEnumerable<FolderItem> items)
        {
            if (!items.Any()) return;
            if (items.Any(e => !FileIO.ExistsPath(e.TargetPath.SimplePath))) return;

            FolderItem? next = null;
            FolderItem? currentBook = items.FirstOrDefault(e => e.TargetPath.SimplePath == BookHub.Current.Address);

            if (Config.Current.Bookshelf.IsOpenNextBookWhenRemove && currentBook != null)
            {
                var index = GetFolderItemIndex(currentBook);
                if (index >= 0)
                {
                    next = _folderCollection?
                        .Skip(index)
                        .Concat(_folderCollection.Take(index).Reverse())
                        .Where(e => !items.Contains(e))
                        .FirstOrDefault();
                }
            }

            var entries = items.Select(e => ArchiveEntryUtility.CreateTemporaryEntry(e.TargetPath.SimplePath)).ToList();
            var removed = await ConfirmFileIO.DeleteAsync(entries, Properties.TextResources.GetString("FileDeleteBookDialog.Title"), null);
            if (removed && _folderCollection != null)
            {
                var removes = items.Where(e => !FileIO.ExistsPath(e.TargetPath.SimplePath)).ToList();
                foreach (var item in removes)
                {
                    _folderCollection.RequestDelete(item.TargetPath);
                }

                if (next != null && !_folderCollection.IsEmpty())
                {
                    SelectedItem = next;
                    LoadBook(SelectedItem);
                }
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _updateFolderCancellationTokenSource.Cancel();
                    _updateFolderCancellationTokenSource.Dispose();

                    _cruiseFolderCancellationTokenSource.Cancel();
                    _cruiseFolderCancellationTokenSource.Dispose();

                    _folderCollection?.Dispose();

                    _disposables.Dispose();

                    //_searchKeyword.Dispose();
                    _searchEngine.Dispose();
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

    }

}
