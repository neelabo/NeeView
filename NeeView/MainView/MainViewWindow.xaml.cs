using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{

    /// <summary>
    /// MainViewWindow.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class MainViewWindow : Window, INotifyPropertyChanged, IDpiScaleProvider, IHasWindowController, INotifyMouseHorizontalWheelChanged, IMainViewWindow, IWindowProcedure
    {
        private readonly DpiScaleProvider _dpiProvider = new();
        private readonly WindowStateManager _windowStateManager;
        private bool _canHideMenu;
        private readonly WindowProcedure _windowProcedure;
        private readonly WindowController _windowController;
        private RoutedCommandBinding? _routedCommandBinding;
        private readonly WeakBindableBase<MainViewConfig> _mainViewConfig;
        private Locker.Key? _referenceSizeLockLey;
        private readonly MouseActivate _mouseActivate;

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

            _mainViewConfig = new WeakBindableBase<MainViewConfig>(Config.Current.MainView);
            _mainViewConfig.AddPropertyChanged(nameof(MainViewConfig.IsHideTitleBar), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsAutoHide));
                UpdateCaptionBar();
            });

            _mainViewConfig.AddPropertyChanged(nameof(MainViewConfig.IsTopmost), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsTopmost));
            });

            MenuAutoHideDescription = new MainViewMenuAutoHideDescription(this.CaptionBar);

            _referenceSizeLockLey = PageFrameProfile.ReferenceSizeLocker.Lock();

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

            var mouseHorizontalWheel = new MouseHorizontalWheelService(this);
            mouseHorizontalWheel.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(s, e);

            _mouseActivate = new MouseActivate(this);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event MouseWheelEventHandler? MouseHorizontalWheelChanged;

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

            RestoreWindowPlacement(placement);

            _referenceSizeLockLey?.Dispose();
            _referenceSizeLockLey = null;
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
            RaisePropertyChanged(nameof(IsFullScreen));
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
            _mouseActivate.Dispose();

            _routedCommandBinding?.Dispose();
            _routedCommandBinding = null;

            _referenceSizeLockLey?.Dispose();
            _referenceSizeLockLey = null;
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
