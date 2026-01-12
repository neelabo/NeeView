using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class BookshelfConfig : FolderListConfig
    {
        private bool _isVisibleHistoryMark = true;
        private bool _isVisibleBookmarkMark = true;
        private string? _excludePattern;
        private bool _isSyncFolderTree;
        private bool _isCloseBookWhenMove;
        private bool _isOpenNextBookWhenRemove = true;
        private bool _isInsertItem = true;
        private bool _isMultipleRarFilterEnabled;
        private bool _isCruise;
        private bool _isSearchIncludeSubdirectories = true;
        private FolderOrder _defaultFolderOrder;
        private FolderOrder _playlistFolderOrder;
        private FolderSortOrder _folderSortOrder = FolderSortOrder.First;
        private bool _isVisibleItemsCount = true;
        private bool _isVisibleSearchBox = true;

        [JsonInclude, JsonPropertyName(nameof(Home))]
        public string? _home;

        /// <summary>
        /// ホームのパス
        /// </summary>
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string Home
        {
            get { return _home ?? BookshelfFolderList.GetDefaultHomePath(); }
            set { SetProperty(ref _home, (string.IsNullOrWhiteSpace(value) || value.Trim() == BookshelfFolderList.GetDefaultHomePath()) ? null : value.Trim()); }
        }

        /// <summary>
        /// 項目に履歴記号を表示する
        /// </summary>
        [PropertyMember]
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { SetProperty(ref _isVisibleHistoryMark, value); }
        }

        /// <summary>
        /// 項目にブックマーク記号を表示する
        /// </summary>
        [PropertyMember]
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { SetProperty(ref _isVisibleBookmarkMark, value); }
        }

        /// <summary>
        /// フォルダーツリーと連動する
        /// </summary>
        [PropertyMember]
        public bool IsSyncFolderTree
        {
            get { return _isSyncFolderTree; }
            set { SetProperty(ref _isSyncFolderTree, value); }
        }

        /// <summary>
        /// 項目移動したら閲覧中のブックを閉じる
        /// </summary>
        [PropertyMember]
        public bool IsCloseBookWhenMove
        {
            get { return _isCloseBookWhenMove; }
            set { SetProperty(ref _isCloseBookWhenMove, value); }
        }

        /// <summary>
        /// 閲覧中のブックを削除したら項目移動
        /// </summary>
        [PropertyMember]
        public bool IsOpenNextBookWhenRemove
        {
            get { return _isOpenNextBookWhenRemove; }
            set { SetProperty(ref _isOpenNextBookWhenRemove, value); }
        }

        /// <summary>
        /// 追加されたファイルを挿入する？
        /// OFFにするとリスト末尾に追加する
        /// </summary>
        [PropertyMember]
        public bool IsInsertItem
        {
            get { return _isInsertItem; }
            set { SetProperty(ref _isInsertItem, value); }
        }

        /// <summary>
        /// 分割RARファイルの場合、先頭のファイルのみを表示
        /// </summary>
        [PropertyMember]
        public bool IsMultipleRarFilterEnabled
        {
            get { return _isMultipleRarFilterEnabled; }
            set { SetProperty(ref _isMultipleRarFilterEnabled, value); }
        }

        /// <summary>
        /// サブフォルダーを含めた巡回移動
        /// </summary>
        [PropertyMember]
        public bool IsCruise
        {
            get { return _isCruise; }
            set { SetProperty(ref _isCruise, value); }
        }

        /// <summary>
        /// 項目除外パターン
        /// </summary>
        [PropertyMember(IsRegex = true)]
        public string? ExcludePattern
        {
            get { return _excludePattern; }
            set { SetProperty(ref _excludePattern, value); }
        }

        /// <summary>
        /// サブフォルダーを含めた検索を行う
        /// </summary>
        [PropertyMember]
        public bool IsSearchIncludeSubdirectories
        {
            get { return _isSearchIncludeSubdirectories; }
            set { SetProperty(ref _isSearchIncludeSubdirectories, value); }
        }

        /// <summary>
        /// 既定の並び順
        /// </summary>
        [PropertyMember]
        public FolderOrder DefaultFolderOrder
        {
            get { return _defaultFolderOrder; }
            set { SetProperty(ref _defaultFolderOrder, value); }
        }

        /// <summary>
        /// プレイリストの既定の並び順
        /// </summary>
        [PropertyMember]
        public FolderOrder PlaylistFolderOrder
        {
            get { return _playlistFolderOrder; }
            set { SetProperty(ref _playlistFolderOrder, value); }
        }

        /// <summary>
        /// フォルダーの並べ替え順序
        /// </summary>
        [PropertyMember]
        public FolderSortOrder FolderSortOrder
        {
            get { return _folderSortOrder; }
            set { SetProperty(ref _folderSortOrder, value); }
        }

        /// <summary>
        /// コレクションアイテム数の表示
        /// </summary>
        [PropertyMember]
        public bool IsVisibleItemsCount
        {
            get { return _isVisibleItemsCount; }
            set { SetProperty(ref _isVisibleItemsCount, value); }
        }

        /// <summary>
        /// 検索ボックスを表示
        /// </summary>
        public bool IsVisibleSearchBox
        {
            get { return _isVisibleSearchBox; }
            set { SetProperty(ref _isVisibleSearchBox, value); }
        }

        #region Obsolete

        /// <summary>
        /// インクリメンタルサーチ有効
        /// </summary>
        [Obsolete("no used"), Alternative("nv.Config.System.IsIncrementalSearchEnabled", 40, ScriptErrorLevel.Warning, IsFullName = true)] // ver.40
        [JsonIgnore]
        public bool IsIncrementalSearchEnabled
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// ファイルタイプを考慮しない並び替え
        /// </summary>
        [Obsolete("no used"), Alternative(nameof(FolderSortOrder), 44, ScriptErrorLevel.Warning)] // ver.44
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsOrderWithoutFileType
        {
            get { return false; }
            set { FolderSortOrder = value ? FolderSortOrder.None : FolderSortOrder.First; }
        }

        #endregion Obsolete
    }

}


