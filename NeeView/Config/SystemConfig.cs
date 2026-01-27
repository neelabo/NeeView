using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class SystemConfig : BindableBase, ICopyPolicy
    {
        private static readonly string _defaultFileManagerFileArgs = "/select,\"$File\"";
        private static readonly string _defaultFileManagerFolderArgs = "\"$File\"";

        private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private bool _isNetworkEnabled = true;
        private bool _isSettingBackup;
        private bool _isHiddenFileVisible;
        private bool _isFileWriteAccessEnabled = false;
        private string? _language;
        private BookPageCollectMode _bookPageCollectMode = BookPageCollectMode.ImageAndBook;
        private bool _isRemoveConfirmed = true;
        private bool _isRemoveWantNukeWarning;
        private bool _isSyncUserSetting = true;
        private bool _isIgnoreImageDpi = true;
        private string _downloadPath = "";
        private bool _isOpenBookAtCurrentPlace;
        private StringComparerType _stringComparer = StringComparerType.Native;
        private bool _isInputMethodEnabled;
        private DestinationFolderCollection _destinationFolderCollection = new();
        private ExternalAppCollection _externalAppCollection = new() { new ExternalApp() };
        private string? _textEditor;
        private string? _webBrowser;
        private string? _fileManager;
        private bool _isIncrementalSearchEnabled = true;
        private int _searchHistorySize = 8;
        private ArchivePolicy _archiveCopyPolicy = ArchivePolicy.SendExtractFile;
        private TextCopyPolicy _textCopyPolicy = TextCopyPolicy.None;

        [JsonInclude, JsonPropertyName(nameof(DateTimeFormat))]
        public string? _dateTimeFormat;

        [JsonInclude, JsonPropertyName(nameof(TemporaryDirectory))]
        public string? _temporaryDirectory;

        [JsonInclude, JsonPropertyName(nameof(FileManagerFileArgs))]
        public string? _fileManagerFileArgs;

        [JsonInclude, JsonPropertyName(nameof(FileManagerFolderArgs))]
        public string? _fileManagerFolderArgs;


        /// <summary>
        /// 言語
        /// </summary>
        [PropertyStrings]
        public string Language
        {
            get { return _language ?? CultureInfo.CurrentCulture.Name; }
            set { SetProperty(ref _language, value); }
        }

        /// <summary>
        /// 日付フォーマット
        /// </summary>
        [JsonIgnore]
        [PropertyMember]
        [NotNull]
        public string? DateTimeFormat
        {
            get { return _dateTimeFormat ?? DateTimeTools.DefaultDateTimePattern; }
            set { SetProperty(ref _dateTimeFormat, (string.IsNullOrWhiteSpace(value) || value == DateTimeTools.DefaultDateTimePattern) ? null : value); }
        }

        [PropertyMember]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        // ページ収集モード
        [PropertyMember]
        public BookPageCollectMode BookPageCollectMode
        {
            get { return _bookPageCollectMode; }
            set { SetProperty(ref _bookPageCollectMode, value); }
        }

        [PropertyMember]
        public bool IsRemoveConfirmed
        {
            get { return _isRemoveConfirmed; }
            set { SetProperty(ref _isRemoveConfirmed, value); }
        }

        [PropertyMember]
        public bool IsRemoveWantNukeWarning
        {
            get { return _isRemoveWantNukeWarning; }
            set { SetProperty(ref _isRemoveWantNukeWarning, value); }
        }

        // ネットワークアクセス許可
        [PropertyMember]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnabled || Environment.IsAppxPackage; } // Appxは強制ON
            set { SetProperty(ref _isNetworkEnabled, value); }
        }

        // 設定データの同期
        [PropertyMember]
        public bool IsSyncUserSetting
        {
            get { return _isSyncUserSetting; }
            set { SetProperty(ref _isSyncUserSetting, value); }
        }

        // 設定データのバックアップ作成
        [PropertyMember]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Environment.IsAppxPackage; }  // Appxは強制ON
            set { SetProperty(ref _isSettingBackup, value); }
        }

        // 画像のDPI非対応
        [PropertyMember]
        public bool IsIgnoreImageDpi
        {
            get { return _isIgnoreImageDpi; }
            set { SetProperty(ref _isIgnoreImageDpi, value); }
        }

        // テンポラリフォルダーの場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string TemporaryDirectory
        {
            get { return _temporaryDirectory ?? Temporary.TempRootPathDefault; }
            set { SetProperty(ref _temporaryDirectory, (string.IsNullOrWhiteSpace(value) || value.Trim() == Temporary.TempRootPathDefault) ? null : value.Trim()); }
        }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string DownloadPath
        {
            get { return _downloadPath; }
            set { SetProperty(ref _downloadPath, value ?? ""); }
        }

        // 隠しファイルを表示する？
        [PropertyMember]
        public bool IsHiddenFileVisible
        {
            get { return _isHiddenFileVisible; }
            set { SetProperty(ref _isHiddenFileVisible, value); }
        }

        [PropertyMember]
        public bool IsFileWriteAccessEnabled
        {
            get { return _isFileWriteAccessEnabled; }
            set { SetProperty(ref _isFileWriteAccessEnabled, value); }
        }


        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        // TODO: LoadAs のコマンドパラメータにする
        [PropertyMember]
        public bool IsOpenBookAtCurrentPlace
        {
            get { return _isOpenBookAtCurrentPlace; }
            set { SetProperty(ref _isOpenBookAtCurrentPlace, value); }
        }

        // 名前順の種類
        [PropertyMember]
        public StringComparerType StringComparerType
        {
            get { return _stringComparer; }
            set { SetProperty(ref _stringComparer, value); }
        }

        // テキストボックス以外でのIME有効 (現状では非公開)
        [PropertyMember]
        public bool IsInputMethodEnabled
        {
            get { return _isInputMethodEnabled; }
            set { SetProperty(ref _isInputMethodEnabled, value); }
        }


        // コピーまたは移動先フォルダーのリスト
        [PropertyMember]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public DestinationFolderCollection DestinationFolderCollection
        {
            get { return _destinationFolderCollection; }
            set { SetProperty(ref _destinationFolderCollection, value); }
        }

        // 外部実行アプリ設定のリスト
        [PropertyMember]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public ExternalAppCollection ExternalAppCollection
        {
            get { return _externalAppCollection; }
            set { SetProperty(ref _externalAppCollection, value); }
        }

        // テキストエディター
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string? TextEditor
        {
            get { return _textEditor; }
            set { SetProperty(ref _textEditor, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        // ウェブブラウザー
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string? WebBrowser
        {
            get { return _webBrowser; }
            set { SetProperty(ref _webBrowser, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        // ファイルマネージャー
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string? FileManager
        {
            get { return _fileManager; }
            set
            {
                if (SetProperty(ref _fileManager, string.IsNullOrWhiteSpace(value) ? null : value.Trim()))
                {
                    if (_fileManager is null)
                    {
                        FileManagerFileArgs = _defaultFileManagerFileArgs;
                        FileManagerFolderArgs = _defaultFileManagerFolderArgs;
                    }
                }
            }
        }

        // ファイルマネージャーの起動引数 (ファイル)
        [JsonIgnore]
        [PropertyMember]
        public string FileManagerFileArgs
        {
            get { return _fileManagerFileArgs ?? _defaultFileManagerFileArgs; }
            set { SetProperty(ref _fileManagerFileArgs, string.IsNullOrWhiteSpace(value) || value.Trim() == _defaultFileManagerFileArgs ? null : value.Trim()); }
        }

        // ファイルマネージャーの起動引数 (フォルダー)
        [JsonIgnore]
        [PropertyMember]
        public string FileManagerFolderArgs
        {
            get { return _fileManagerFolderArgs ?? _defaultFileManagerFolderArgs; }
            set { SetProperty(ref _fileManagerFolderArgs, string.IsNullOrWhiteSpace(value) || value.Trim() == _defaultFileManagerFolderArgs ? null : value.Trim()); }
        }

        /// <summary>
        /// インクリメンタルサーチ有効
        /// </summary>
        [PropertyMember]
        public bool IsIncrementalSearchEnabled
        {
            get { return _isIncrementalSearchEnabled; }
            set { SetProperty(ref _isIncrementalSearchEnabled, value); }
        }

        /// <summary>
        /// 検索ボックスの履歴数
        /// </summary>
        [PropertyMember]
        public int SearchHistorySize
        {
            get { return _searchHistorySize; }
            set { SetProperty(ref _searchHistorySize, Math.Max(0, value)); }
        }

        // 圧縮ファイルコピーのヒント
        [PropertyMember]
        public ArchivePolicy ArchiveCopyPolicy
        {
            get { return _archiveCopyPolicy; }
            set { SetProperty(ref _archiveCopyPolicy, value); }
        }

        // テキストコピーのヒント
        [PropertyMember]
        public TextCopyPolicy TextCopyPolicy
        {
            get { return _textCopyPolicy; }
            set { SetProperty(ref _textCopyPolicy, value); }
        }

        #region HiddenParameters

        /// <summary>
        /// 読み込まれた設定であるか
        /// </summary>
        /// <remarks>
        /// 初回起動時チェック用
        /// </remarks>
        [JsonIgnore]
        [PropertyMapIgnore]
        public bool IsLoadedSettings { get; set; }

        #endregion

        #region Obsolete

        [Obsolete("Typo"), Alternative(nameof(IsHiddenFileVisible), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public bool IsHiddenFileVisibled
        {
            get { return IsHiddenFileVisible; }
            set { IsHiddenFileVisible = value; }
        }

        [Obsolete("Typo json interface"), PropertyMapIgnore]
        [JsonPropertyName("IsHiddenFileVisibled"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHiddenFileVisibled_Typo
        {
            get { return default; }
            set { IsHiddenFileVisible = value; }
        }

        [Obsolete("Typo"), Alternative(nameof(IsOpenBookAtCurrentPlace), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public bool IsOpenbookAtCurrentPlace
        {
            get { return IsOpenBookAtCurrentPlace; }
            set { IsOpenBookAtCurrentPlace = value; }
        }

        [Obsolete("Typo json interface"), PropertyMapIgnore]
        [JsonPropertyName("IsOpenbookAtCurrentPlace"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsOpenbookAtCurrentPlace_Typo
        {
            get { return default; }
            set { IsOpenBookAtCurrentPlace = value; }
        }

        [Obsolete("Typo"), Alternative(nameof(DestinationFolderCollection), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public DestinationFolderCollection DestinationFodlerCollection
        {
            get { return DestinationFolderCollection; }
            set { DestinationFolderCollection = value; }
        }

        [Obsolete("Typo json interface"), PropertyMapIgnore]
        [JsonPropertyName("DestinationFodlerCollection"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DestinationFolderCollection? DestinationFodlerCollection_Typo
        {
            get { return default; }
            set { DestinationFolderCollection = value ?? new(); }
        }

        // カスタム自然順ソート
        [Obsolete("no used"), Alternative(nameof(StringComparerType), 44, ScriptErrorLevel.Warning)] // ver.44
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsNaturalSortEnabled
        {
            get { return false; }
            set { StringComparerType = value ? StringComparerType.Natural : StringComparerType.Native; }
        }

        #endregion Obsolete
    }


    public enum StringComparerType
    {
        Ascii = 0,
        Native = 1,
        Natural = 2,
    }
}
