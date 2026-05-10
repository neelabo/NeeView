using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{

    /// <summary>
    /// MainViewWindow.xaml の相互作用ロジック
    /// </summary>
    [INotifyPropertyChanged]
    public partial class MainViewWindow : Window, IDpiScaleProvider, IHasWindowController, IMainViewWindow, IWindowProcedure
    {
        private readonly DpiScaleProvider _dpiProvider = new();
        private readonly WindowStateManager _windowStateManager;
        private bool _canHideMenu;
        private readonly WindowProcedure _windowProcedure;
        private readonly WindowController _windowController;
        private RoutedCommandBinding? _routedCommandBinding;
        private Locker.Key? _referenceSizeLocKey;
        private readonly MouseActivate _mouseActivate;
        private readonly DisposableCollection _disposables = new();

        public MainViewWindow()
        {
            InitializeComponent();
            WindowChromeTools.SetWindowChromeSource(this);

            this.DataContext = this;

            this.SetBinding(MainViewWindow.TitleProperty, new Binding(nameof(WindowTitle.Title)) { Source = WindowTitle.Current });

            _windowProcedure = new WindowProcedure(this);

            _windowStateManager = new WindowStateManager(this);
            _windowStateManager.StateChanged += WindowStateManager_StateChanged;

            _windowController = new MainViewWindowController(this, _windowStateManager);

            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);
            _disposables.Add(_routedCommandBinding);

            _disposables.Add(Config.Current.MainView.SubscribePropertyChanged(nameof(MainViewConfig.IsHideTitleBar), (s, e) =>
            {
                OnPropertyChanged(nameof(IsAutoHide));
                UpdateCaptionBar();
            }));

            _disposables.Add(Config.Current.MainView.SubscribePropertyChanged(nameof(MainViewConfig.IsTopmost), (s, e) =>
            {
                OnPropertyChanged(nameof(IsTopmost));
            }));

            MenuAutoHideDescription = new MainViewMenuAutoHideDescription(this.CaptionBar);

            _referenceSizeLocKey = PageFrameProfile.ReferenceSizeLocker.Lock();
            _disposables.Add(_referenceSizeLocKey);

            this.SourceInitialized += MainViewWindow_SourceInitialized;
            this.Loaded += MainViewWindow_Loaded;
            this.DpiChanged += MainViewWindow_DpiChanged;
            this.Activated += MainViewWindow_Activated;
            this.Closing += MainViewWindow_Closing;
            this.Closed += MainViewWindow_Closed;

            // key event for window
            this.PreviewKeyDown += MainViewWindow_PreviewKeyDown;
            this.KeyDown += MainViewWindow_KeyDown;

            UpdateCaptionBar();

            MouseHorizontalWheelService.SubscribeHorizontalWheelEvent(this);

            _mouseActivate = new MouseActivate(this);
            _disposables.Add(_mouseActivate);
        }


        public WindowProcedure WindowProcedure => _windowProcedure;

        public WindowController WindowController => _windowController;

        public WindowStateManager WindowStateManager => _windowStateManager;

        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public BasicAutoHideDescription MenuAutoHideDescription { get; private set; }


        public bool IsTopmost
        {
            get { return Config.Current.MainView.IsTopmost; }
            set { Config.Current.MainView.IsTopmost = value; }
        }

        public bool IsAutoHide
        {
            get { return Config.Current.MainView.IsHideTitleBar; }
            set { Config.Current.MainView.IsHideTitleBar = value; }
        }

        public bool IsAutoStretch
        {
            get { return Config.Current.MainView.IsAutoStretch; }
            set { Config.Current.MainView.IsAutoStretch = value; }
        }

        public bool CanHideMenu
        {
            get { return _canHideMenu; }
            set { SetProperty(ref _canHideMenu, value); }
        }

        public bool IsFullScreen
        {
            get { return _windowStateManager.IsFullScreen; }
            set { _windowStateManager.SetFullScreen(value); }
        }


        private void MainViewWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var placement = Config.Current.MainView.WindowPlacement;
            if (placement.IsValid() && placement.WindowState == WindowState.Minimized)
            {
                placement = placement.WithState(WindowState.Normal);
            }

            RestoreWindowResumeState(Config.Current.MainView.LastState);
            RestoreWindowPlacement(placement);

            if (_referenceSizeLocKey is not null)
            {
                _disposables.Remove(_referenceSizeLocKey);
                _referenceSizeLocKey.Dispose();
                _referenceSizeLocKey = null;
            }
        }

        private void MainViewWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            _dpiProvider.SetDipScale(VisualTreeHelper.GetDpi(this));
        }

        private void MainViewWindow_DpiChanged(object? sender, DpiChangedEventArgs e)
        {
            _dpiProvider.SetDipScale(e.NewDpi);
        }

        private void MainViewWindow_Activated(object? sender, EventArgs e)
        {
            RoutedCommandTable.Current.UpdateInputGestures();
        }

        private void MainViewWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                PendingItemManager.Current.Cancel();
            }
        }

        private void MainViewWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // ESCキーでウィンドウを閉じる
            if (e.Key == Key.Escape)
            {
                SystemCommands.CloseWindow(this);
                e.Handled = true;
            }
        }

        private void WindowStateManager_StateChanged(object? sender, EventArgs e)
        {
            UpdateCaptionBar();
            OnPropertyChanged(nameof(IsFullScreen));
        }

        private void MainViewWindow_Closing(object? sender, CancelEventArgs e)
        {
            // ウィンドウを閉じる処理は最小化に置き換える
            if (Config.Current.MainView.IsFloating && !Config.Current.MainView.IsFloatingEndWhenClosed)
            {
                SystemCommands.MinimizeWindow(this);
                e.Cancel = true;
            }
        }

        private void MainViewWindow_Closed(object? sender, EventArgs e)
        {
            _disposables.Dispose();
        }

        private void UpdateCaptionBar()
        {
            if (Config.Current.MainView.IsHideTitleBar || _windowStateManager.IsFullScreen)
            {
                this.CanHideMenu = true;
                Grid.SetRow(this.CaptionBar, 1);
            }
            else
            {
                this.CanHideMenu = false;
                Grid.SetRow(this.CaptionBar, 0);
            }
        }

        public DpiScale GetDpiScale()
        {
            return _dpiProvider.DpiScale;
        }

        public WindowStateEx StoreWindowResumeState()
        {
            return _windowStateManager.IsFullScreen ? _windowStateManager.ResumeState : WindowStateEx.Normal;
        }

        public void RestoreWindowResumeState(WindowStateEx state)
        {
            _windowStateManager.ResumeState = state;
        }

        public WindowPlacement StoreWindowPlacement()
        {
            return _windowStateManager.StoreWindowPlacement(withAeroSnap: true);
        }

        public void RestoreWindowPlacement(WindowPlacement placement)
        {
            _windowStateManager.RestoreWindowPlacement(placement);
        }

        private void StretchWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.MainViewSocket.Content is MainView mainView)
            {
                mainView.StretchWindow();
            }
        }
    }


    public class MainViewMenuAutoHideDescription : BasicAutoHideDescription
    {
        private readonly CaptionBar _captionBar;

        public MainViewMenuAutoHideDescription(CaptionBar captionBar) : base(captionBar)
        {
            _captionBar = captionBar;
        }

        public override bool IsVisibleLocked()
        {
            if (_captionBar.IsMaximizeButtonMouseOver)
            {
                return true;
            }

            return base.IsVisibleLocked();
        }
    }
}
