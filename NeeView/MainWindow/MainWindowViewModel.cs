using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// MainWindow : ViewModel
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        private bool _initialized;
        private MainWindowModel _model;
        private readonly MainViewComponent _viewComponent;
        private Thickness _sidePanelMargin;
        private double _canvasWidth;
        private double _canvasHeight;
        private bool _isFilmStripFocusRequest;
        private readonly Messenger _visibleAtOnceMessenger;


        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindowViewModel(MainWindowModel model, Messenger visibleAtOnceMessenger)
        {
            _viewComponent = MainViewComponent.Current;

            MenuAutoHideDescription = new MenuAutoHideDescription(MainWindow.Current.LayerMenuSocket, MainWindow.Current.SidePanelFrame);
            StatusAutoHideDescription = new StatusAutoHideDescription(MainWindow.Current.LayerStatusArea, MainWindow.Current.SidePanelFrame);
            FilmStripAutoHideDescription = new StatusAutoHideDescription(MainWindow.Current.LayerFilmStripSocket, MainWindow.Current.SidePanelFrame);

            _visibleAtOnceMessenger = visibleAtOnceMessenger;
            _visibleAtOnceMessenger.Subscribe<MenuVisibleAtOnceMessage>(VisibleAtOnce);
            _visibleAtOnceMessenger.Subscribe<StatusVisibleAtOnceMessage>(VisibleAtOnce);
            _visibleAtOnceMessenger.Subscribe<FilmStripVisibleAtOnceMessage>(VisibleAtOnce);
            _visibleAtOnceMessenger.Subscribe<AllVisibleAtOnceMessage>(VisibleAtOnce);
            _visibleAtOnceMessenger.Subscribe<FilmStripFocusAtOnceMessage>(FocusAtOnce);

            // main window model
            _model = model;

            _model.AddPropertyChanged(nameof(_model.CanHideMenu),
                (s, e) => UpdateSidePanelMargin());

            _model.AddPropertyChanged(nameof(_model.CanHidePageSlider),
                (s, e) => UpdateSidePanelMargin());

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.ConflictTopMargin),
                (s, e) => UpdateSidePanelMargin());

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.ConflictBottomMargin),
                (s, e) => UpdateSidePanelMargin());

            _model.FocusMainViewCall += Model_FocusMainViewCall;

            MainWindow.Current.Activated +=
                (s, e) => RaisePropertyChanged(nameof(IsMenuBarActive));

            MainWindow.Current.Deactivated +=
                (s, e) => RaisePropertyChanged(nameof(IsMenuBarActive));

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsTopmost),
                (s, e) => RaisePropertyChanged(nameof(IsTopmost)));

            BookHub.Current.BookChanged +=
                (s, e) => CommandManager.InvalidateRequerySuggested();

            Environment.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    SaveData.Current.DisableSave(); // 保存禁止
                    App.Current.MainWindow.Close();
                };

            PageTitle.Current.SubscribePropertyChanged(nameof(PageTitle.Title),
                (s, e) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(Title))));

            // TODO: アプリの初期化処理で行うべき
            // ダウンロードフォルダー生成
            if (!FileIO.DirectoryExists(Temporary.Current.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.Current.TempDownloadDirectory);
            }
        }


        public event EventHandler? FocusMainViewCall;


        public bool IsClosing { get; set; }

        public ImageSource? WindowIcon => null;

        public string Title => PageTitle.Current.Title;

        public bool IsTopmost
        {
            get { return Config.Current.Window.IsTopmost; }
            set { Config.Current.Window.IsTopmost = value; }
        }

        // for Binding
        public MainWindowController WindowController => _model.WindowController;
        public WindowTitle WindowTitle => WindowTitle.Current;
        public PageTitle PageTitle => PageTitle.Current;
        public FilmStrip FilmStrip => FilmStrip.Current;
        public ImageEffect ImageEffect => ImageEffect.Current;
        public MouseInput MouseInput => _viewComponent.MouseInput;
        public InfoMessage InfoMessage => InfoMessage.Current;
        public SidePanelFrame SidePanel => SidePanelFrame.Current;
        public ToastService ToastService => ToastService.Current;
        public App App => App.Current;
        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;


        /// <summary>
        /// MainWindow Model
        /// </summary>
        public MainWindowModel Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        /// <summary>
        /// メニュの自動非表示ON/OFFによるサイドパネル上部の余白
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _sidePanelMargin; }
            set { SetProperty(ref _sidePanelMargin, value); }
        }

        /// <summary>
        /// キャンバス幅。SidePanelから引き渡される
        /// </summary>
        public double CanvasWidth
        {
            get { return _canvasWidth; }
            set { SetProperty(ref _canvasWidth, value); }
        }

        /// <summary>
        /// キャンバス高
        /// </summary>
        public double CanvasHeight
        {
            get { return _canvasHeight; }
            set { SetProperty(ref _canvasHeight, value); }
        }

        /// <summary>
        /// Menu用AutoHideBehavior補足
        /// </summary>
        public MenuAutoHideDescription MenuAutoHideDescription { get; }

        /// <summary>
        /// FilmStrep, Slider 用 AutoHideBehavior 補足
        /// </summary>
        public BasicAutoHideDescription StatusAutoHideDescription { get; }

        public BasicAutoHideDescription FilmStripAutoHideDescription { get; }

        public bool IsMenuBarActive => MainWindow.Current.IsActive;

        /// <summary>
        /// フィルムストリップ フォーカス要求
        /// </summary>
        public bool IsFilmStripFocusRequest
        {
            get { return _isFilmStripFocusRequest; }
            set { SetProperty(ref _isFilmStripFocusRequest, value); }
        }


        /// <summary>
        /// 初期化
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            await _model.LoadedAsync();
            _model.ContentRendered();
        }

        /// <summary>
        /// Window OnLoaded
        /// </summary>
        public void Loaded()
        {
        }

        /// <summary>
        /// Window OnContentRendered
        /// </summary>
        public async Task ContentRenderedAsync()
        {
            await InitializeAsync();
        }

        /// <summary>
        /// ウィンドウがアクティブ化したときの処理
        /// </summary>
        public void Activated()
        {
            if (IsClosing) return;

            RoutedCommandTable.Current.UpdateInputGestures();
            SaveDataSync.Current.ResetWatcher();
        }

        /// <summary>
        /// ウィンドウが非アクティブ化したときの処理
        /// </summary>
        public void Deactivated()
        {
            if (IsClosing) return;

            _ = ArchiveManager.Current.UnlockAllArchivesAsync();
        }

        /// <summary>
        /// メインビューフォーカス要求
        /// </summary>
        private void Model_FocusMainViewCall(object? sender, EventArgs e)
        {
            FocusMainViewCall?.Invoke(sender, e);
        }

        public void VisibleAtOnce(MenuVisibleAtOnceMessage e)
        {
            MenuAutoHideDescription.VisibleOnce(e.Visibility);
        }

        public void VisibleAtOnce(StatusVisibleAtOnceMessage e)
        {
            StatusAutoHideDescription.VisibleOnce(e.Visibility, e.FocusFilmStrip ? OnVisibleOnceFilmStrip : null);
        }

        public void VisibleAtOnce(FilmStripVisibleAtOnceMessage e)
        {
            FilmStripAutoHideDescription.VisibleOnce(e.Visibility, e.FocusFilmStrip ? OnVisibleOnceFilmStrip : null);
        }

        public void VisibleAtOnce(AllVisibleAtOnceMessage e)
        {
            MenuAutoHideDescription.VisibleOnce(e.Visibility);
            StatusAutoHideDescription.VisibleOnce(e.Visibility);
            FilmStripAutoHideDescription.VisibleOnce(e.Visibility);
        }

        private void OnVisibleOnceFilmStrip(bool isVisible)
        {
            if (isVisible)
            {
                // 可能ならばフィルムストリップをフォーカスする
                IsFilmStripFocusRequest = true;
            }
        }

        private void FocusAtOnce(FilmStripFocusAtOnceMessage e)
        {
            IsFilmStripFocusRequest = true;
        }

        /// <summary>
        /// 自動表示パネルをすぐ閉じる
        /// </summary>
        public void AllPanelHideAtOnce()
        {
            _model.AllPanelHideAtOnce();
        }

        /// <summary>
        /// サイドパネルの上下スペース幅を更新
        /// </summary>
        private void UpdateSidePanelMargin()
        {
            SidePanelMargin = new Thickness(0, _model.CanHideMenu ? Config.Current.Panels.ConflictTopMargin : 0, 0, _model.CanHidePageSlider ? Config.Current.Panels.ConflictBottomMargin : 0);
        }
    }


    public record MenuVisibleAtOnceMessage(VisibilityRequest Visibility);
    public record StatusVisibleAtOnceMessage(VisibilityRequest Visibility, bool FocusFilmStrip);
    public record FilmStripVisibleAtOnceMessage(VisibilityRequest Visibility, bool FocusFilmStrip);
    public record AllVisibleAtOnceMessage(VisibilityRequest Visibility);

    public record FilmStripFocusAtOnceMessage();
}
