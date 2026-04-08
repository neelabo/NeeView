using Generator.Equals;
using NeeView.Text;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class BookshelfConfig : FolderListConfig
    {
        [IgnoreEquality]
        private readonly RegexCollectionCache _excludeRegexCache = new();

        [DefaultEquality] private bool _isVisibleHistoryMark = true;
        [DefaultEquality] private bool _isVisibleBookmarkMark = true;
        [DefaultEquality] private StringCollection _excludeRegexes = new();
        [DefaultEquality] private bool _isSyncFolderTree;
        [DefaultEquality] private bool _isSyncFolderTreeAuto;
        [DefaultEquality] private bool _isCloseBookWhenMove;
        [DefaultEquality] private bool _isOpenNextBookWhenRemove = true;
        [DefaultEquality] private bool _isInsertItem = true;
        [DefaultEquality] private bool _isMultipleRarFilterEnabled;
        [DefaultEquality] private bool _isCruise;
        [DefaultEquality] private bool _isSearchIncludeSubdirectories = true;
        [DefaultEquality] private FolderOrder _defaultFolderOrder;
        [DefaultEquality] private FolderOrder _playlistFolderOrder;
        [DefaultEquality] private FolderSortOrder _folderSortOrder = FolderSortOrder.First;
        [DefaultEquality] private bool _isVisibleItemsCount = true;
        [DefaultEquality] private bool _isVisibleSearchBox = true;
        [DefaultEquality] private string? _home;
        [DefaultEquality] private string? _folderConfigFilePath;
        [DefaultEquality] private string? _quickAccessFilePath;

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

        [JsonPropertyName(nameof(Home))]
        [PropertyMapIgnore]
        public string? HomeRaw
        {
            get { return _home; }
            set { _home = value; }
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
        /// 同期ボタンでフォルダーツリーと同期する
        /// </summary>
        [PropertyMember]
        public bool IsSyncFolderTree
        {
            get { return _isSyncFolderTree; }
            set { SetProperty(ref _isSyncFolderTree, value); }
        }

        /// <summary>
        /// フォルダーツリーと自動同期する
        /// </summary>
        [PropertyMember]
        public bool IsSyncFolderTreeAuto
        {
            get { return _isSyncFolderTreeAuto; }
            set { SetProperty(ref _isSyncFolderTreeAuto, value); }
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
        /// ページ除外パターン (正規表現)
        /// </summary>
        [PropertyMember]
        public StringCollection ExcludeRegexes
        {
            get { return _excludeRegexes; }
            set { SetProperty(ref _excludeRegexes, value); }
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

        /// <summary>
        /// フォルダー設定の保存場所
        /// </summary>
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string FolderConfigFilePath
        {
            get { return _folderConfigFilePath ?? SaveDataProfile.DefaultFolderConfigFilePath; }
            set { SetProperty(ref _folderConfigFilePath, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveDataProfile.DefaultFolderConfigFilePath) ? null : value.Trim()); }
        }

        [JsonPropertyName(nameof(FolderConfigFilePath))]
        [PropertyMapIgnore]
        public string? FolderConfigFilePathRaw
        {
            get { return _folderConfigFilePath; }
            set { _folderConfigFilePath = value; }
        }

        /// <summary>
        /// クイックアクセス設定の保存場所
        /// </summary>
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string QuickAccessFilePath
        {
            get { return _quickAccessFilePath ?? SaveDataProfile.DefaultQuickAccessFilePath; }
            set { SetProperty(ref _quickAccessFilePath, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveDataProfile.DefaultQuickAccessFilePath) ? null : value.Trim()); }
        }

        [JsonPropertyName(nameof(QuickAccessFilePath))]
        [PropertyMapIgnore]
        public string? QuickAccessFilePathRaw
        {
            get { return _quickAccessFilePath; }
            set { _quickAccessFilePath = value; }
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

        /// <summary>
        /// 項目除外パターン
        /// </summary>
        [Obsolete("no used"), Alternative(nameof(ExcludeRegexes), 46, ScriptErrorLevel.Warning)] // ver.46
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? ExcludePattern
        {
            get { return null; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ExcludeRegexes = new StringCollection();
                }
                else
                {
                    ExcludeRegexes = new StringCollection([value]);
                }
            }
        }

        #endregion Obsolete

        /// <summary>
        /// 除外パターンを正規表現で取得する
        /// </summary>
        /// <returns></returns>
        public List<Regex> GetExcludeRegexes()
        {
            return _excludeRegexCache.GetRegexs(ExcludeRegexes);
        }
    }

}


