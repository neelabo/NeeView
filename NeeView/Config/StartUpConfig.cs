using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class StartUpConfig : BindableBase
    {
        [DefaultEquality] private string? _lastBookPath;
        [DefaultEquality] private string? _lastFolderPath;
        [DefaultEquality] private BookMemento? _lastBook;
        [DefaultEquality] private BookshelfFolderMemento? _lastFolder;
        [DefaultEquality] private bool _isSplashScreenEnabled = true;
        [DefaultEquality] private bool _isMultiBootEnabled;
        [DefaultEquality] private bool _isRestoreWindowPlacement = true;
        [DefaultEquality] private bool _isRestoreSecondWindowPlacement = true;
        [DefaultEquality] private bool _isRestoreFullScreen;
        [DefaultEquality] private bool _isOpenLastBook;
        [DefaultEquality] private bool _isOpenLastFolder;
        [DefaultEquality] private bool _isAutoPlaySlideShow;


        // スプラッシュスクリーン
        [PropertyMember]
        public bool IsSplashScreenEnabled
        {
            get { return _isSplashScreenEnabled; }
            set { SetProperty(ref _isSplashScreenEnabled, value); }
        }

        // 多重起動を許可する
        [PropertyMember]
        public bool IsMultiBootEnabled
        {
            get { return _isMultiBootEnabled; }
            set { SetProperty(ref _isMultiBootEnabled, value); }
        }

        // ウィンドウ座標を復元する
        [PropertyMember]
        public bool IsRestoreWindowPlacement
        {
            get { return _isRestoreWindowPlacement; }
            set { SetProperty(ref _isRestoreWindowPlacement, value); }
        }

        // 複数ウィンドウの座標復元
        [PropertyMember]
        public bool IsRestoreSecondWindowPlacement
        {
            get { return _isRestoreSecondWindowPlacement; }
            set { SetProperty(ref _isRestoreSecondWindowPlacement, value); }
        }

        // フルスクリーン状態を復元する
        [PropertyMember]
        public bool IsRestoreFullScreen
        {
            get { return _isRestoreFullScreen; }
            set { SetProperty(ref _isRestoreFullScreen, value); }
        }

        // 前回開いていたブックを開く
        [PropertyMember]
        public bool IsOpenLastBook
        {
            get { return _isOpenLastBook; }
            set { SetProperty(ref _isOpenLastBook, value); }
        }

        // 前回開いていた本棚を復元
        [PropertyMember]
        public bool IsOpenLastFolder
        {
            get { return _isOpenLastFolder; }
            set { SetProperty(ref _isOpenLastFolder, value); }
        }

        /// <summary>
        /// 起動時にスライドショーを開始する
        /// </summary>
        [PropertyMember]
        public bool IsAutoPlaySlideShow
        {
            get { return _isAutoPlaySlideShow; }
            set { SetProperty(ref _isAutoPlaySlideShow, value); }
        }


        #region HiddenParameters

        [Obsolete, PropertyMapIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LastBookPath
        {
            get { return IsOpenLastBook ? _lastBookPath : null; }
            set { SetProperty(ref _lastBookPath, value); }
        }

        [Obsolete, PropertyMapIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LastFolderPath
        {
            get { return IsOpenLastFolder ? _lastFolderPath : null; }
            set { SetProperty(ref _lastFolderPath, value); }
        }

        [PropertyMapIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public BookMemento? LastBook
        {
            get { return _lastBook; }
            set { SetProperty(ref _lastBook, value); }
        }

        [PropertyMapIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BookMementoSlim? LastBookV2
        {
            get { return BookMementoSlim.Create(LastBook); }
            set { LastBook = value?.ToBookMemento(); }
        }

        [PropertyMapIgnore]
        public BookshelfFolderMemento? LastFolder
        {
            get { return _lastFolder; }
            set { SetProperty(ref _lastFolder, value); }
        }

        #endregion HiddenParameters
    }
}
