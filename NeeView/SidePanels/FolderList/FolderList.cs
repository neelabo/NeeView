﻿// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    //
    public class SelectedChangedEventArgs : EventArgs
    {
        public bool IsFocus { get; set; }
    }

    //
    public class BusyChangedEventArgs : EventArgs
    {
        public bool IsBusy { get; set; }

        public BusyChangedEventArgs(bool isBusy)
        {
            this.IsBusy = isBusy;
        }
    }


    //
    public class FolderList : BindableBase
    {
        public static FolderList Current { get; private set; }

        #region Fields

        private BookHub _bookHub;

        /// <summary>
        /// そのフォルダーで最後に選択されていた項目の記憶
        /// </summary>
        private Dictionary<string, string> _lastPlaceDictionary = new Dictionary<string, string>();

        /// <summary>
        /// 更新フラグ
        /// </summary>
        private bool _isDarty;

        /// <summary>
        /// 検索エンジン
        /// </summary>
        private SearchEngine _searchEngine;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookHub"></param>
        /// <param name="folderPanel"></param>
        public FolderList(BookHub bookHub, FolderPanelModel folderPanel)
        {
            Current = this;

            this.FolderPanel = folderPanel;
            _bookHub = bookHub;

            _bookHub.FolderListSync += async (s, e) => await SyncWeak(e);
            _bookHub.HistoryChanged += (s, e) => RefleshIcon(e.Key);
            _bookHub.BookmarkChanged += (s, e) => RefleshIcon(e.Key);
        }

        #endregion

        #region Events

        public event EventHandler PlaceChanged;

        //
        public event EventHandler SelectedChanging;
        public event EventHandler<SelectedChangedEventArgs> SelectedChanged;

        // FolderCollection総入れ替え
        public event EventHandler CollectionChanged;

        // 検索ボックスにフォーカスを
        public event EventHandler SearchBoxFocus;

        /// <summary>
        /// リスト更新処理中イベント
        /// </summary>
        public event EventHandler<BusyChangedEventArgs> BusyChanged;

        #endregion

        #region Properties

        //
        public FolderPanelModel FolderPanel { get; private set; }

        /// <summary>
        /// PanelListItemStyle property.
        /// </summary>
        private PanelListItemStyle _panelListItemStyle;
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// フォルダーアイコン表示位置
        /// </summary>
        private FolderIconLayout _folderIconLayout = FolderIconLayout.Default;
        public FolderIconLayout FolderIconLayout
        {
            get { return _folderIconLayout; }
            set { if (_folderIconLayout != value) { _folderIconLayout = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleHistoryMark property.
        /// </summary>
        private bool _isVisibleHistoryMark = true;
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { if (_isVisibleHistoryMark != value) { _isVisibleHistoryMark = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleBookmarkMark property.
        /// </summary>
        private bool _isVisibleBookmarkMark = true;
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { if (_isVisibleBookmarkMark != value) { _isVisibleBookmarkMark = value; RaisePropertyChanged(); } }
        }

        /// </summary>
        private string _home;
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFの場合はリスト末尾に追加する
        /// </summary>
        public bool IsInsertItem { get; set; } = true;

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        private FolderCollection _folderCollection;
        public FolderCollection FolderCollection
        {
            get { return _folderCollection; }
            set
            {
                if (_folderCollection != value)
                {
                    _folderCollection?.Dispose();
                    _folderCollection = value;
                    CollectionChanged?.Invoke(this, null);
                    RaisePropertyChanged(nameof(FolderOrder));
                    RaisePropertyChanged(nameof(IsFolderSearchCollection));
                    RaisePropertyChanged(nameof(IsFolderSearchEnabled));
                }
            }
        }

        /// <summary>
        /// 検索リスト？
        /// </summary>
        public bool IsFolderSearchCollection => FolderCollection is FolderSearchCollection;

        /// <summary>
        /// 検索許可？
        /// </summary>
        public bool IsFolderSearchEnabled => _place != null && !(FolderCollection is FolderArchiveCollection);

        /// <summary>
        /// SelectedItem property.
        /// </summary>
        private FolderItem _selectedItem;
        public FolderItem SelectedItem
        {
            get { return _selectedItem; }
            set { if (_selectedItem != value) { _selectedItem = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 現在のフォルダー
        /// </summary>
        private string _place;

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        public History<string> History { get; private set; } = new History<string>();

        /// <summary>
        /// IsFolderSearchVisible property.
        /// </summary>
        private bool _IsFolderSearchVisible = true;
        public bool IsFolderSearchBoxVisible
        {
            get { return _IsFolderSearchVisible; }
            set { if (_IsFolderSearchVisible != value) { _IsFolderSearchVisible = value; RaisePropertyChanged(); if (!_IsFolderSearchVisible) this.SearchKeyword = ""; } }
        }

        /// <summary>
        /// SearchKeyword property.
        /// </summary>
        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { if (_searchKeyword != value) { _searchKeyword = value; RaisePropertyChanged(); var task = UpdateFolderCollectionAsync(false); } }
        }

        /// <summary>
        /// SearchHistory property.
        /// </summary>
        private ObservableCollection<string> _searchHistory = new ObservableCollection<string>();
        public ObservableCollection<string> SearchHistory
        {
            get { return _searchHistory; }
            set { if (_searchHistory != value) { _searchHistory = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 補正されたHOME取得
        /// </summary>
        /// <returns></returns>
        public string GetFixedHome()
        {
            if (Directory.Exists(_home)) return _home;

            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture)) return myPicture;

            // 救済措置。
            return Environment.CurrentDirectory;
        }

        /// <summary>
        /// ふさわしい選択項目インデックスを取得
        /// </summary>
        /// <param name="path">選択したいパス</param>
        /// <returns></returns>
        internal int FixedIndexOfPath(string path)
        {
            var index = this.FolderCollection.IndexOfPath(path);
            return index < 0 ? 0 : index;
        }

        //
        internal FolderItem FixedItem(string path)
        {
            return this.FolderCollection.FirstOrDefault(path) ?? this.FolderCollection.FirstOrDefault();
        }

        /// <summary>
        /// フォルダー状態保存
        /// </summary>
        /// <param name="folder"></param>
        private void SavePlace(FolderItem folder)
        {
            if (folder == null || folder.ParentPath == null) return;
            _lastPlaceDictionary[folder.ParentPath] = folder.Path;
        }

        /// <summary>
        /// 項目変更前通知
        /// </summary>
        public void RaiseSelectedItemChanging()
        {
            SelectedChanging?.Invoke(this, null);
        }

        /// <summary>
        /// 項目変更後通知
        /// </summary>
        /// <param name="isFocus"></param>
        public void RaiseSelectedItemChanged(bool isFocus = false)
        {
            SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
        }


        /// <summary>
        /// 場所の初期化。
        /// nullを指定した場合、HOMEフォルダに移動。
        /// </summary>
        /// <param name="place"></param>
        public void ResetPlace(string place)
        {
            var task = SetPlaceAsync(place ?? GetFixedHome(), null, FolderSetPlaceOption.IsUpdateHistory);
        }

        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="place">フォルダーパス</param>
        /// <param name="select">初期選択項目</param>
        public async Task SetPlaceAsync(string place, string select, FolderSetPlaceOption options)
        {
            // 現在フォルダーの情報を記憶
            SavePlace(GetFolderItem(0));

            // 初期項目
            if (select == null && place != null)
            {
                _lastPlaceDictionary.TryGetValue(place, out select);
            }

            if (options.HasFlag(FolderSetPlaceOption.IsTopSelect))
            {
                select = null;
            }

            // 更新が必要であれば、新しいFolderListBoxを作成する
            if (CheckFolderListUpdateneNcessary(place, options.HasFlag(FolderSetPlaceOption.ClearSearchKeyword)))
            {
                _isDarty = false;

                // 検索エンジン停止
                _searchEngine?.Dispose();
                _searchEngine = null;

                // 検索キーワードクリア
                if (this.FolderCollection == null || place != _place || options.HasFlag(FolderSetPlaceOption.ClearSearchKeyword))
                {
                    _searchKeyword = "";
                    RaisePropertyChanged(nameof(SearchKeyword));
                }

                // 場所変更
                _place = place;

                // FolderCollection 更新
                await UpdateFolderCollectionAsync(true);

                this.SelectedItem = FixedItem(select);

                RaiseSelectedItemChanged(options.HasFlag(FolderSetPlaceOption.IsFocus));

                // 最終フォルダー更新
                BookHistory.Current.LastFolder = _place;

                // 履歴追加
                if (options.HasFlag(FolderSetPlaceOption.IsUpdateHistory))
                {
                    if (_place != this.History.GetCurrent())
                    {
                        this.History.Add(_place);
                    }
                }
            }
            else
            {
                // 選択項目のみ変更
                this.SelectedItem = FixedItem(select);
            }

            // 変更通知
            PlaceChanged?.Invoke(this, null);
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        /// <param name="place"></param>
        /// <param name="releaseSearchMode">検索モード解除</param>
        /// <returns></returns>
        private bool CheckFolderListUpdateneNcessary(string place, bool releaseSearchMode)
        {
            return (_isDarty || this.FolderCollection == null || place != this.FolderCollection.Place || (releaseSearchMode && this.FolderCollection is FolderSearchCollection));
        }


        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        private FolderCollection CreateEntryCollection(string place)
        {
            try
            {
                return new FolderEntryCollection(place);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                return new FolderEntryCollection(Environment.CurrentDirectory);
            }
        }

        /// <summary>
        /// FolderCollection作成(検索結果)
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        private FolderCollection CreateSearchCollection(string place, NeeLaboratory.IO.Search.SearchResultWatcher searchResult)
        {
            return new FolderSearchCollection(place, searchResult);
        }


        /// <summary>
        /// FolderCollection作成(書庫内アーカイブリスト)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="archiver"></param>
        /// <returns></returns>
        private FolderCollection CreateArchiveCollection(string place, Archiver archiver)
        {
            return new FolderArchiveCollection(place, archiver);
        }


        /// <summary>
        /// フォルダーリスト項目変更前処理
        /// 項目が削除される前に有効な選択項目に変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderCollection_Deleting(object sender, System.IO.FileSystemEventArgs e)
        {
            if (e.ChangeType != System.IO.WatcherChangeTypes.Deleted) return;

            var item = this.FolderCollection.FirstOrDefault(e.FullPath);
            if (item != this.SelectedItem) return;

            RaiseSelectedItemChanging();
            this.SelectedItem = GetNeighbor(item);
            RaiseSelectedItemChanged();
        }

        // となりを取得
        public FolderItem GetNeighbor(FolderItem item)
        {
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



        /// <summary>
        /// フォルダーリスト更新
        /// </summary>
        /// <param name="force">必要が無い場合も更新する</param>
        public async Task RefleshAsync(bool force)
        {
            if (this.FolderCollection == null) return;

            _isDarty = force || this.FolderCollection.IsDarty();

            await SetPlaceAsync(_place, null, FolderSetPlaceOption.IsUpdateHistory);
        }



        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem GetFolderItem(int offset)
        {
            if (this.FolderCollection?.Items == null) return null;

            int index = this.FolderCollection.Items.IndexOf(this.SelectedItem);
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
        }


        /// <summary>
        /// 現在開いているフォルダーで更新(弱)
        /// e.isKeepPlaceが有効の場合、フォルダーは移動せず現在選択項目のみの移動を試みる
        /// </summary>
        /// <param name="e"></param>
        public async Task SyncWeak(FolderListSyncArguments e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (this.FolderCollection == null || this.FolderCollection.Contains(e.Path)) return;
            }

            var options = FolderSetPlaceOption.IsUpdateHistory;
            await SetPlaceAsync(e.Parent, e.Path, options);
        }

        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefleshIcon(string path)
        {
            this.FolderCollection?.RefleshIcon(path);
        }


        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (this.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(FolderCollection.Items, QueueElementPriority.FolderThumbnail, start, count, margin, direction);
            }
        }


        // ブックの読み込み
        public void LoadBook(string path)
        {
            BookLoadOption option = BookLoadOption.SkipSamePlace | (this.FolderCollection.FolderParameter.IsFolderRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            LoadBook(path, option);
        }

        // ブックの読み込み
        public void LoadBook(string path, BookLoadOption option)
        {
            _bookHub.RequestLoad(path, null, option, false);
        }


        // 現在の場所のフォルダーの並び順
        public FolderOrder FolderOrder
        {
            get { return GetFolderOrder(); }
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            if (FolderCollection == null) return;
            this.FolderCollection.FolderParameter.FolderOrder = folderOrder;
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// フォルダーの並びを取得
        /// </summary>
        public FolderOrder GetFolderOrder()
        {
            if (this.FolderCollection == null) return default(FolderOrder);
            return this.FolderCollection.FolderParameter.FolderOrder;
        }


        /// <summary>
        /// フォルダーの並びを順番に切り替える
        /// </summary>
        public void ToggleFolderOrder()
        {
            if (this.FolderCollection == null) return;
            this.FolderCollection.FolderParameter.FolderOrder.GetToggle();
            RaisePropertyChanged(nameof(FolderOrder));
        }

        // 次のフォルダーに移動
        public async Task NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_bookHub.IsBusy()) return; // 相対移動の場合はキャンセルしない
            var result = await MoveFolder(+1, option);
            if (result != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "次のブックはありません");
            }
        }

        // 前のフォルダーに移動
        public async Task PrevFolder(BookLoadOption option = BookLoadOption.None)
        {
            if (_bookHub.IsBusy()) return; // 相対移動の場合はキャンセルしない
            var result = await MoveFolder(-1, option);
            if (result != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "前のブックはありません");
            }
        }


        /// <summary>
        /// コマンドの「前のフォルダーに移動」「次のフォルダーへ移動」に対応
        /// </summary>
        public async Task<bool> MoveFolder(int direction, BookLoadOption options)
        {
            var item = this.GetFolderItem(direction);
            if (item != null)
            {
                await SetPlaceAsync(_place, item.Path, FolderSetPlaceOption.IsUpdateHistory);
                _bookHub.RequestLoad(item.TargetPath, null, options, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 検索ボックスにフォーカス要求
        /// </summary>
        public void RaiseSearchBoxFocus()
        {
            SearchBoxFocus?.Invoke(this, null);
        }

        /// <summary>
        /// 検索キーワードの正規化
        /// </summary>
        /// <returns></returns>
        private string GetFixedSearchKeyword() => _searchKeyword?.Trim();


        /// <summary>
        /// コレクション更新
        /// </summary>
        /// <param name="isForce">強制更新</param>
        /// <returns></returns>
        public async Task UpdateFolderCollectionAsync(bool isForce)
        {
            try
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(true));
                await UpdateFolderCollectionAsyncInner(isForce);
            }
            finally
            {
                BusyChanged?.Invoke(this, new BusyChangedEventArgs(false));
            }
        }

        /// <summary>
        /// コレクション更新
        /// </summary>
        /// <param name="isForce"></param>
        /// <returns></returns>
        private async Task UpdateFolderCollectionAsyncInner(bool isForce)
        {
            // 検索処理は停止
            _searchEngine?.CancelSearch();

            var keyword = GetFixedSearchKeyword();

            if (!string.IsNullOrEmpty(keyword))
            {
                await UpdateSearchFolderCollectionAsync(keyword, isForce);
            }
            else if (_place == null || Directory.Exists(_place))
            {
                await UpdateEntryFolderCollectionAsync(isForce);
            }
            else
            {
                await UpdateArchiveFolderCollectionAsync(isForce);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isForce"></param>
        /// <returns></returns>
        public async Task UpdateEntryFolderCollectionAsync(bool isForce)
        {
            // 同じリストは作らない
            if (!isForce && this.FolderCollection != null && this.FolderCollection.Place == _place && this.FolderCollection is FolderEntryCollection) return;

            var collection = await Task.Run(() => CreateEntryCollection(_place));
            InitializeCollectionEvent(collection);

            this.FolderCollection = collection;
        }

        /// <summary>
        /// 検索結果リストとしてFolderListを更新
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="isForce"></param>
        /// <returns></returns>
        public async Task UpdateSearchFolderCollectionAsync(string keyword, bool isForce)
        {
            // 同じリストは作らない
            if (!isForce && this.FolderCollection != null && (this.FolderCollection is FolderSearchCollection e && e.SearchKeyword == keyword)) return;

            try
            {
                _searchEngine = _searchEngine ?? new SearchEngine(_place);

                var option = new NeeLaboratory.IO.Search.SearchOption() { AllowFolder = true, IsOptionEnabled = true };
                var result = await _searchEngine.SearchAsync(keyword, option);

                var collection = CreateSearchCollection(_place, result);
                InitializeCollectionEvent(collection);

                this.FolderCollection = collection;
            }
            catch (OperationCanceledException)
            {
                ////Debug.WriteLine($"Search: Canceled.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Search Exception: {ex.Message}");
                _searchEngine?.Dispose();
                _searchEngine = null;
            }
        }

        /// <summary>
        /// アーカイブフォルダー
        /// </summary>
        /// <param name="isForce"></param>
        /// <returns></returns>
        public async Task UpdateArchiveFolderCollectionAsync(bool isForce)
        {
            // 同じリストは作らない
            if (!isForce && this.FolderCollection != null && this.FolderCollection.Place == _place && this.FolderCollection is FolderArchiveCollection) return;

            try
            {
                using (var entry = await ArchiveFileSystem.CreateArchiveEntry(_place, CancellationToken.None))
                {
                    var collection = CreateArchiveCollection(_place, await ArchiverManager.Current.CreateArchiverAsync(entry, false, false, CancellationToken.None));
                    InitializeCollectionEvent(collection);
                    this.FolderCollection = collection;
                }
            }
            // アーカイブパスが展開できない場合、実在パスでの展開を行う
            catch (FileNotFoundException)
            {
                _place = ArchiveFileSystem.GetExistDirectoryName(_place);
                await UpdateFolderCollectionAsyncInner(isForce);
            }
        }


        /// <summary>
        /// フォルダーコレクションのイベント設定
        /// </summary>
        /// <param name="collection"></param>
        private void InitializeCollectionEvent(FolderCollection collection)
        {
            collection.ParameterChanged += async (s, e) => await RefleshAsync(true);
            collection.Deleting += FolderCollection_Deleting;
        }

        /// <summary>
        /// 検索履歴更新
        /// </summary>
        public void UpdateSearchHistory()
        {
            var keyword = GetFixedSearchKeyword();
            if (string.IsNullOrWhiteSpace(keyword)) return;

            if (this.SearchHistory.Count <= 0)
            {
                this.SearchHistory.Add(keyword);
            }
            else if (this.SearchHistory.First() != keyword)
            {
                int index = this.SearchHistory.IndexOf(keyword);
                if (index > 0)
                {
                    this.SearchHistory.Move(index, 0);
                }
                else
                {
                    this.SearchHistory.Insert(0, keyword);
                }
            }

            while (this.SearchHistory.Count > 6)
            {
                this.SearchHistory.RemoveAt(this.SearchHistory.Count - 1);
            }
        }




        #endregion

        #region Commands


        public void SetHome_Executed()
        {
            if (_bookHub == null) return;
            this.Home = _place;
        }

        //
        public async void MoveToHome_Executed()
        {
            if (_bookHub == null) return;

            var place = GetFixedHome();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.IsTopSelect | FolderSetPlaceOption.ClearSearchKeyword);
        }


        //
        public async void MoveTo_Executed(string path)
        {
            await this.SetPlaceAsync(path, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }

        //
        public bool MoveToPrevious_CanExecutre()
        {
            return this.History.CanPrevious();
        }

        //
        public async void MoveToPrevious_Executed()
        {
            if (!this.History.CanPrevious()) return;

            var place = this.History.GetPrevious();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus);
            this.History.Move(-1);
        }

        //
        public bool MoveToNext_CanExecute()
        {
            return this.History.CanNext();
        }

        //
        public async void MoveToNext_Executed()
        {
            if (!this.History.CanNext()) return;

            var place = this.History.GetNext();
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus);
            this.History.Move(+1);
        }

        //
        public async void MoveToHistory_Executed(KeyValuePair<int, string> item)
        {
            var place = this.History.GetHistory(item.Key);
            await SetPlaceAsync(place, null, FolderSetPlaceOption.IsFocus);
            this.History.SetCurrent(item.Key + 1);
        }

        //
        public bool MoveToParent_CanExecute()
        {
            return (_place != null);
        }

        //
        public async void MoveToParent_Execute()
        {
            if (_place == null) return;

            var parent = this.FolderCollection is FolderArchiveCollection collection
                ? collection.GetParentPlace()
                : Path.GetDirectoryName(_place);

            await SetPlaceAsync(parent, _place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }

        //
        public async void Sync_Executed()
        {
            string place = _bookHub?.Book?.Place;

            if (place != null)
            {
                var parent = _bookHub?.Book?.Archiver?.Parent?.FullPath ?? LoosePath.GetDirectoryName(place);

                _isDarty = true; // 強制更新
                await SetPlaceAsync(parent, place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);

                RaiseSelectedItemChanged(true);
            }
            else if (_place != null)
            {
                _isDarty = true; // 強制更新
                await SetPlaceAsync(_place, null, FolderSetPlaceOption.IsFocus);

                RaiseSelectedItemChanged(true);
            }
        }

        //
        public void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public FolderIconLayout FolderIconLayout { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string Home { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("フォルダーリスト追加ファイルは挿入", Tips = "フォルダーリストで追加されたファイルを現在のソート順で挿入します。\nFalseのときはリストの終端に追加します。")]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsFolderSearchBoxVisible { get; set; } = true;

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                IsFolderSearchBoxVisible = true;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            memento.FolderIconLayout = this.FolderIconLayout;
            memento.IsVisibleHistoryMark = this.IsVisibleHistoryMark;
            memento.IsVisibleBookmarkMark = this.IsVisibleBookmarkMark;
            memento.Home = this.Home;
            memento.IsInsertItem = this.IsInsertItem;
            memento.IsFolderSearchBoxVisible = this.IsFolderSearchBoxVisible;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.FolderIconLayout = memento.FolderIconLayout;
            this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;
            this.Home = memento.Home;
            this.IsInsertItem = memento.IsInsertItem;
            this.IsFolderSearchBoxVisible = memento.IsFolderSearchBoxVisible;

            // Preference反映
            ///RaisePropertyChanged(nameof(FolderIconLayout));
        }

        #endregion
    }




    /// <summary>
    /// 旧フォルダーリスト設定。
    /// 互換性のために残してあります
    /// </summary>
    [DataContract]
    public class FolderListSetting
    {
        [DataMember]
        public bool IsVisibleHistoryMark { get; set; }

        [DataMember]
        public bool IsVisibleBookmarkMark { get; set; }
    }
}
