//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Data;
using NeeView.Native;
using NeeView.Setting;
using NeeView.Threading;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IDpiScaleProvider, IHasWindowController, INotifyMouseHorizontalWheelChanged, IHasRenameManager, IMainViewWindow
    {
        private static MainWindow? _current;
        public static MainWindow Current => _current ?? throw new InvalidOperationException();

        // インスタンス保持用
        [SuppressMessage("CodeQuality", "IDE0052:読み取られていないプライベート メンバーを削除", Justification = "<保留中>")]
        private readonly RoutedCommandBinding _routedCommandBinding;

        private readonly MainWindowViewModel _vm;
        private readonly MainViewComponent _viewComponent;
        private readonly DpiScaleProvider _dpiProvider = new();

        private readonly WindowStateManager _windowStateManager;
        private readonly MainWindowController _windowController;

        private readonly MediaControl _mediaControl;

        public MainWindow()
        {
            NVInterop.NVFpReset();

            InitializeComponent();
            WindowChromeTools.SetWindowChromeSource(this);

            // TextBox の ContextMenu のスタイルを変更する ... やりすぎ？
            // ThemeProfile.InitializeEditorContextMenuStyle(this);

            Debug.WriteLine($"App.MainWindow.Initialize: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            if (_current is not null) throw new InvalidOperationException();
            _current = this;

            DragDropHelper.AttachDragOverTerminator(this);

            // Window状態初期化
            InitializeWindowShapeSnap();

            _windowStateManager = new WindowStateManager(this);
            _windowController = new MainWindowController(this, _windowStateManager);

            ContextMenuWatcher.Initialize();

            var mouseHorizontalWheel = new MouseHorizontalWheelService(this);
            mouseHorizontalWheel.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(s, e);

            // 固定画像初期化
            ThumbnailResource.InitializeStaticImages();
            _ = FileIconCollection.Current.InitializeAsync();

            // FpReset 念のため
            NVInterop.NVFpReset();

            // Drag&Drop設定
            //ContentDropManager.Current.SetDragDropEvent(MainView);

            // ViewComponent
            MainViewComponent.Initialize();
            _viewComponent = MainViewComponent.Current;

            // ViewComponent Mouse/TouchCommand
            RoutedCommandTable.Current.AddMouseInput(_viewComponent.MouseInput);
            RoutedCommandTable.Current.AddTouchInput(_viewComponent.TouchInput);

            MainViewManager.Initialize(_viewComponent, this.MainViewSocket);

            MainWindowModel.Initialize(_windowController);

            // MainWindow : ViewModel
            _vm = new MainWindowViewModel(MainWindowModel.Current);
            this.DataContext = _vm;

            _vm.FocusMainViewCall += (s, e) => _viewComponent.RaiseFocusMainViewRequest();

            // コマンド初期化
            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);

            // MainWindow MouseCommand Terminator
            var mouseContext = new MouseInputContext(this, null, MouseGestureCommandCollection.Current, null, null, null, null, null)
            {
                IsGestureEnabled = false,
                IsLeftButtonDownEnabled = false,
                IsRightButtonDownEnabled = false,
                IsVerticalWheelEnabled = false,
                IsMouseEventTerminated = false
            };
            RoutedCommandTable.Current.AddMouseInput(new MouseInput(mouseContext));

            // サイドパネル初期化
            CustomLayoutPanelManager.Initialize();

            // 各コントロールとモデルを関連付け
            _mediaControl = new MediaControl();
            this.PageSliderView.Source = PageSlider.Current;
            this.MediaControlView.Source = _mediaControl;
            this.ThumbnailListArea.Source = ThumbnailList.Current;
            this.MenuBar.Source = new MenuBar(_windowStateManager);
            this.AddressBar.Source = new AddressBar();

            _vm.MenuAutoHideDescription.SetMenuBar(this.MenuBar.Source);

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenu),
                (s, e) => DirtyMenuAreaLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHideMenu),
                (s, e) => DirtyMenuAreaLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHidePageSlider),
                (s, e) => DirtyPageSliderLayout());

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsEnabled),
                (s, e) => DirtyPageSliderLayout());

            Config.Current.FilmStrip.AddPropertyChanged(nameof(FilmStripConfig.IsEnabled),
                (s, e) => DirtyThumbnailListLayout());

            Config.Current.FilmStrip.AddPropertyChanged(nameof(FilmStripConfig.IsHideFilmStrip),
                (s, e) => DirtyThumbnailListLayout());

            ThumbnailList.Current.VisibleEvent +=
                ThumbnailList_Visible;

            _viewComponent.PageFrameBoxPresenter.SubscribePageFrameBoxChanged(
                (s, e) => DirtyPageSliderLayout());

            // # ここで？
            _viewComponent.PageFrameBoxPresenter.SubscribeViewContentChanged((s, e) =>
            {
                var viewContent = e.ViewContents.FirstOrDefault() as IHasViewContentMediaPlayer;
                //this.MediaControlView.SetSource(viewContent);
                var player = viewContent?.Player;
                if (player is not null && MainViewComponent.Current.PageFrameBoxPresenter.IsMedia)
                {
                    _mediaControl.RaiseContentChanged(this, new MediaPlayerChanged(player, false) { IsMainMediaPlayer = true });
                }
                else
                {
                    _mediaControl.RaiseContentChanged(this, new MediaPlayerChanged() { IsMainMediaPlayer = true });
                }
            });



            _windowController.SubscribePropertyChanged(nameof(MainWindowController.AutoHideMode),
                (s, e) => AutoHideModeChanged());

            // initialize routed commands
            RoutedCommandTable.Current.UpdateInputGestures();

            // watch menu bar visibility
            this.MenuArea.IsVisibleChanged += (s, e) => Config.Current.MenuBar.IsVisible = this.MenuArea.IsVisible;

            // watch slider visibility
            this.SliderArea.IsVisibleChanged += (s, e) => Config.Current.Slider.IsVisible = this.SliderArea.IsVisible;

            // moue event for window
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
            this.PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            this.PreviewStylusDown += MainWindow_PreviewStylusDown;

            // mouse activate
            this.MouseDown += MainWindow_MouseDown;

            // key event for window
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.PreviewKeyUp += MainWindow_PreviewKeyUp;
            this.KeyDown += MainWindow_KeyDown;

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.CloseAll();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.CloseAll();

            // frame event
            CompositionTarget.Rendering += OnRendering;

            // message layer space
            InitializeMessageLayerSpace();

            // page caption
            InitializePageCaption();

            // side panel quick hide
            this.MainViewPanelRect.PreviewMouseDown += (s, e) => _vm.AllPanelHideAtOnce();
            this.MainViewPanelRect.PreviewMouseWheel += (s, e) => _vm.AllPanelHideAtOnce();

            // 開発用初期化
            Debug_Initialize();

            Debug.WriteLine($"App.MainWindow.Initialize.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }


        /// <summary>
        /// マウス水平ホイールイベント
        /// </summary>
        [Subscribable]
        public event MouseWheelEventHandler? MouseHorizontalWheelChanged;

        /// <summary>
        /// キー入力イベント購読
        /// </summary>

        public IDisposable SubscribePreviewKeyDown(KeyEventHandler handler)
        {
            PreviewKeyDown += handler;
            return new AnonymousDisposable(() => PreviewKeyDown -= handler);
        }



        public WindowStateManager WindowStateManager => _windowStateManager;

        public WindowController WindowController => _windowController;



        #region 初期化処理

        /// <summary>
        /// Window状態初期設定 
        /// </summary>
        private static void InitializeWindowShapeSnap()
        {
            var state = Config.Current.Window.State;

            if (App.Current.Option.IsResetPlacement == SwitchOption.on)
            {
                state = WindowStateEx.Normal;
            }
            else if (state == WindowStateEx.FullScreen)
            {
                state = Config.Current.StartUp.IsRestoreFullScreen
                    ? WindowStateEx.FullScreen
                    : Config.Current.Window.LastState == WindowStateEx.Maximized ? WindowStateEx.Maximized : WindowStateEx.Normal;
            }
            else if (state == WindowStateEx.Minimized)
            {
                state = WindowStateEx.Normal;
            }
            else if (!Config.Current.StartUp.IsRestoreWindowPlacement)
            {
                state = WindowStateEx.Normal;
            }

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Environment.IsSecondProcess && !Config.Current.StartUp.IsRestoreSecondWindowPlacement)
            {
                state = WindowStateEx.Normal;
            }

            if (App.Current.Option.WindowState.HasValue)
            {
                state = App.Current.Option.WindowState.ToWindowStateEx();
            }

            Config.Current.Window.State = state;
        }

        #endregion


        #region マウスによるウィンドウアクティブ監視

        public bool IsMouseActivate { get; private set; }

        public void SetMouseActivate()
        {
            if (!IsActive)
            {
                IsMouseActivate = true;
                _ = ResetMouseActivateAsync(100);
            }
        }

        private async Task ResetMouseActivateAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
            IsMouseActivate = false;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseActivate = false;
        }

        #endregion

        #region ウィンドウ状態コマンド

        /// <summary>
        /// ウィンドウ最小化コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ////FocusMainView();
            _viewComponent.RaiseFocusMainViewRequest();
            SystemCommands.MinimizeWindow(this);
        }

        /// <summary>
        /// 通常ウィンドウ化コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestoreWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        /// <summary>
        /// ウィンドウ最大化コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizeWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        /// <summary>
        /// ウィンドウ終了コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region ウィンドウ座標保存

        // ウィンドウ座標保存
        public void StoreWindowPlacement()
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            try
            {
                Config.Current.Window.LastState = _windowStateManager.ResumeState;
                Config.Current.Window.WindowPlacement = _windowStateManager.StoreWindowPlacement(Config.Current.Window.IsRestoreAeroSnapPlacement);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // ウィンドウ座標復元
        public void RestoreWindowPlacement()
        {
            // 座標を復元しない
            if (App.Current.Option.IsResetPlacement == SwitchOption.on || !Config.Current.StartUp.IsRestoreWindowPlacement) return;

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Environment.IsSecondProcess && !Config.Current.StartUp.IsRestoreSecondWindowPlacement) return;

            var state = Config.Current.Window.State;
            var placement = Config.Current.Window.WindowPlacement;

            if (placement != null && placement.IsValid())
            {
                placement = placement.WithState(state.ToWindowState(), state.IsFullScreen());

                _windowStateManager.ResumeState = Config.Current.Window.LastState;
                _windowStateManager.RestoreWindowPlacement(placement);
            }
            else
            {
                _windowStateManager.SetWindowState(state);
            }
        }

        #endregion

        #region ウィンドウイベント処理

        /// <summary>
        /// フレーム処理
        /// </summary>
        private void OnRendering(object? sender, EventArgs e)
        {
            LayoutFrame();
        }


        // ウィンドウソース初期化後イベント
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            Debug.WriteLine($"App.MainWindow.SourceInitialized: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            // Chrome の情報を最新にする
            _windowController.Refresh();

            // ウィンドウ座標の復元
            // NOTE: NativeMethods.SetWindowPlacement() を呼ぶとLoadedイベントが発生する。WindowPlacementの処理順番に注意
            RestoreWindowPlacement();

            Debug.WriteLine($"App.MainWindow.SourceInitialized.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウ表示開始
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"App.MainWindow.Loaded: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            MessageDialog.OwnerWindow = this;

            _dpiProvider.SetDipScale(VisualTreeHelper.GetDpi(this));

            PendingItemManager.Initialize(this);

            MainViewManager.Current.Update(false);

            // レイアウト更新
            DirtyWindowLayout();

            // WinProc登録
            SystemDeviceWatcher.Current.Initialize(this);

            _vm.Loaded();

            if (InputGestureDisplayString.ErrorMessages.Count > 0)
            {
                ToastService.Current.Show(new Toast(string.Join(System.Environment.NewLine, InputGestureDisplayString.ErrorMessages), "Text resource errors", ToastIcon.Warning));
            }

            App.Current.IsMainWindowLoaded = true;

            // activate
            if (this.WindowState != WindowState.Minimized)
            {
                this.Activate();
            }

            Trace.WriteLine($"App.MainWindow.Loaded.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウコンテンツ表示開始
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine($"App.MainWindow.ContentRendered: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            _vm.ContentRendered();

            // focus
            if (this.WindowState != WindowState.Minimized)
            {
                this.Focus();
            }

            Debug.WriteLine($"App.MainWindow.ContentRendered.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            // 初回起動ダイアログ
            if (!Config.Current.System.IsLoadedSettings)
            {
                AppDispatcher.BeginInvoke(() => WelcomeDialog.ShowDialog(this));
            }
        }

        // ウィンドウアクティブ
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            ////SetCursorVisible(true);
            _vm.Activated();
        }

        // ウィンドウ非アクティブ
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            ////SetCursorVisible(true);
            _vm.Deactivated();
        }

        /// <summary>
        /// ウィンドウ状態変更イベント処理
        /// </summary>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();

            if (e.Key == Key.Escape)
            {
                PendingItemManager.Current.Cancel();
            }
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // ALTキーのメニュー操作無効 (Alt+F4は常に有効)
            if (!Config.Current.Command.IsAccessKeyEnabled && (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey != Key.F4))
            {
                e.Handled = true;
            }
        }

        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // NOTE: フルスクリーンのときに左上の(0,0)座標が取得できなくなるので下記処理を無効化 (2021-01-04)
            // NOTE: 下記処理をしないことによる不具合が確認できないため、無効化してしばらく様子見する
#if false
            // WindowChromeでのウィンドウ移動直後のマウス座標が不正(0,0)になるのようなので、この場合は無効にする
            var windowPoint = e.GetPosition(this);
            if (windowPoint.X == 0.0 && windowPoint.Y == 0.0)
            {
                Debug.WriteLine($"Wrong cursor position!");
                e.Handled = true;
            }
#endif
        }

        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
        }


        /// <summary>
        /// DPI変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            var isChanged = _dpiProvider.SetDipScale(e.NewDpi);
            if (!isChanged) return;

            this.MenuBar.WindowCaptionButtons.UpdateStrokeThickness(e.NewDpi);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Trace.WriteLine($"Window.Closing:");
            FinalizeMainWindow();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Trace.WriteLine($"Window.Closed:");
            MessageDialog.OwnerWindow = null;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void FinalizeMainWindow()
        {
            if (_vm.IsClosing) return;
            _vm.IsClosing = true;

            // ブック読み込み要求無効化
            BookHub.Current.IsEnabled = false;

            // 非同期処理の完了を待つ
            WorkingProgressWatcher.Current.WaitWithDialog(this);

            // レンダリングイベント購読停止
            CompositionTarget.Rendering -= OnRendering;

            // コマンド停止
            _routedCommandBinding.Dispose();
            RoutedCommandTable.Current.Dispose();

            // コンソールウィンドウを閉じる
            ConsoleWindowManager.Current.Dispose();

            // 設定ウィンドウの保存動作を無効化
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.AllowSave = false;
            }

            // パネルレイアウトの保存
            CustomLayoutPanelManager.Current?.Store();
            CustomLayoutPanelManager.Current?.SetIsStoreEnabled(false);

            // メインビューの保存
            MainViewManager.Current?.Store();
            MainViewManager.Current?.SetIsStoreEnabled(false);

            // ウィンドウ座標の保存
            StoreWindowPlacement();
        }

        #endregion

        #region メニューエリア、ステータスエリアマウスオーバー監視

        public bool IsMenuAreaMouseOver()
        {
            return this.MenuArea.IsMouseOver;
        }

        public bool IsStatusAreaMouseOver()
        {
            return this.DockStatusArea.IsMouseOver || this.LayerStatusArea.IsMouseOver;
        }

        #endregion

        #region レイアウト管理

        private bool _isDirtyMenuAreaLayout;
        private bool _isDirtyPageSliderLayout;
        private bool _isDirtyThumbnailListLayout;

        /// <summary>
        /// 自動非表示モードが変更されたときの処理
        /// </summary>
        private void AutoHideModeChanged()
        {
            DirtyWindowLayout();

            // 解除でフォーカスが表示されたパネルに移動してしまう現象を回避
            if (!_windowController.AutoHideMode && Config.Current.Panels.IsHidePanelInAutoHideMode)
            {
                _viewComponent.RaiseFocusMainViewRequest();
            }
        }

        /// <summary>
        /// レイアウト更新要求
        /// </summary>
        private void DirtyWindowLayout()
        {
            _isDirtyMenuAreaLayout = true;
            _isDirtyPageSliderLayout = true;
            _isDirtyThumbnailListLayout = true;
        }

        /// <summary>
        /// メニューエリアレイアウト更新要求
        /// </summary>
        private void DirtyMenuAreaLayout()
        {
            _isDirtyMenuAreaLayout = true;
        }

        /// <summary>
        /// スライダーレイアウト更新要求
        /// </summary>
        private void DirtyPageSliderLayout()
        {
            _isDirtyPageSliderLayout = true;
            _isDirtyThumbnailListLayout = true; // フィルムストリップも更新
        }

        /// <summary>
        /// フィルムストリップ更新要求
        /// </summary>
        private void DirtyThumbnailListLayout()
        {
            _isDirtyThumbnailListLayout = true;
        }


        /// <summary>
        /// レイアウト更新フレーム処理
        /// </summary>
        private void LayoutFrame()
        {
            UpdateMenuAreaLayout();
            UpdateStatusAreaLayout();
        }

        /// <summary>
        /// メニューエリアレイアウト更新。
        /// </summary>
        private void UpdateMenuAreaLayout()
        {
            if (!_isDirtyMenuAreaLayout) return;
            _isDirtyMenuAreaLayout = false;

            // menu hide
            bool isMenuDock = !MainWindowModel.Current.CanHideMenu;

            if (isMenuDock)
            {
                this.LayerMenuSocket.Content = null;
                this.DockMenuSocket.Content = this.MenuArea;
            }
            else
            {
                this.DockMenuSocket.Content = null;
                this.LayerMenuSocket.Content = this.MenuArea; ;
            }
        }

        /// <summary>
        /// ステータスエリアレイアウト更新。
        /// </summary>
        private void UpdateStatusAreaLayout()
        {
            UpdatePageSliderLayout();
            UpdateThumbnailListLayout();
        }

        /// <summary>
        /// ページスライダーレイアウト更新。
        /// </summary>
        private void UpdatePageSliderLayout()
        {
            if (!_isDirtyPageSliderLayout) return;
            _isDirtyPageSliderLayout = false;

            // menu hide
            bool isPageSliderDock = !MainWindowModel.Current.CanHidePageSlider;

            if (isPageSliderDock)
            {
                this.LayerPageSliderSocket.Content = null;
                this.DockPageSliderSocket.Content = this.SliderArea;
                this.MediaControlView.IsBackgroundOpacityEnabled = false;
                this.PageSliderView.IsBackgroundOpacityEnabled = false;
            }
            else
            {
                this.DockPageSliderSocket.Content = null;
                this.LayerPageSliderSocket.Content = this.SliderArea;
                this.MediaControlView.IsBackgroundOpacityEnabled = true;
                this.PageSliderView.IsBackgroundOpacityEnabled = true;
            }

            // visibility
            if (_viewComponent.PageFrameBoxPresenter.IsMedia)
            {
                this.MediaControlView.Visibility = Visibility.Visible;
                this.PageSliderView.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MediaControlView.Visibility = Visibility.Collapsed;
                this.PageSliderView.Visibility = Config.Current.Slider.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// フィルムストリップレイアウト更新
        /// </summary>
        private void UpdateThumbnailListLayout()
        {
            if (!_isDirtyThumbnailListLayout) return;
            _isDirtyThumbnailListLayout = false;

            bool isPageSliderDock = !MainWindowModel.Current.CanHidePageSlider;
            bool isThumbnailListDock = !Config.Current.FilmStrip.IsHideFilmStrip && isPageSliderDock;

            if (isThumbnailListDock)
            {
                this.LayerThumbnailListSocket.Content = null;
                this.DockThumbnailListSocket.Content = this.ThumbnailListArea;
                this.ThumbnailListArea.IsBackgroundOpacityEnabled = false;
            }
            else
            {
                this.DockThumbnailListSocket.Content = null;
                this.LayerThumbnailListSocket.Content = this.ThumbnailListArea;
                this.ThumbnailListArea.IsBackgroundOpacityEnabled = true;
            }

            // フィルムストリップ
            this.ThumbnailListArea.Visibility = Config.Current.FilmStrip.IsEnabled && !PageFrameBoxPresenter.Current.IsMedia ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ThumbnailList_Visible(object? sender, VisibleEventArgs e)
        {
            _vm.StatusAutoHideDescription.VisibleOnce(true);
            _vm.ThumbnailListAutoHideDescription.VisibleOnce(true);

            if (e.IsFocus)
            {
                ThumbnailList.Current.FocusAtOnce();
            }
        }

        private void InitializeMessageLayerSpace()
        {
            this.DockStatusArea.SizeChanged += (s, e) => { if (e.HeightChanged) { UpdateMessageLayerSpace(); } };

            UpdateMessageLayerSpace();
        }

        private void UpdateMessageLayerSpace()
        {
            this.MessageLayerSpace.Height = Math.Max(this.DockStatusArea.ActualHeight, 30.0) + 10.0;
        }

        #endregion レイアウト管理

        #region ページタイトル管理

        private DelayVisibility _pageCaptionVisibility;

        [MemberNotNull(nameof(_pageCaptionVisibility))]
        private void InitializePageCaption()
        {
            this.DockStatusArea.SizeChanged +=
                (s, e) => UpdatePageCaptionLayout();

            this.DockStatusArea.MouseEnter +=
                (s, e) => UpdatePageCaptionVisibility();

            this.DockStatusArea.MouseLeave +=
                (s, e) => UpdatePageCaptionVisibility();

            this.LayerStatusArea.IsVisibleChanged +=
                (s, e) => UpdatePageCaptionLayout();

            this.LayerStatusArea.SizeChanged +=
                (s, e) => UpdatePageCaptionLayout();

            this.LayerStatusArea.MouseEnter +=
                (s, e) => UpdatePageCaptionVisibility();

            this.LayerStatusArea.MouseLeave +=
                (s, e) => UpdatePageCaptionVisibility();

            _vm.AddPropertyChanged(nameof(_vm.Title),
                (s, e) => UpdatePageCaptionVisibility());

            ContextMenuWatcher.ContextMenuClosing +=
                (s, e) => UpdatePageCaptionVisibility();

            _pageCaptionVisibility = new DelayVisibility();
            _pageCaptionVisibility.Changed +=
                (s, e) => PageCaptionVisibility_Changed(s, e);

            this.PageCaption.Visibility = _pageCaptionVisibility.Visibility;
            UpdatePageCaptionLayout();
            UpdatePageCaptionVisibility();
        }

        private void PageCaptionVisibility_Changed(object? sender, EventArgs e)
        {
            this.PageCaption.Visibility = _pageCaptionVisibility.Visibility;
        }

        private void UpdatePageCaptionLayout()
        {
            const double margin = 5.0;
            var space = Math.Max(this.LayerStatusArea.IsVisible ? this.LayerStatusArea.ActualHeight : 0.0, this.DockStatusArea.ActualHeight);
            this.PageCaption.Margin = new Thickness(margin, margin, margin, margin + space);
        }

        private void UpdatePageCaptionVisibility()
        {
            if (ContextMenuWatcher.TargetElement != null)
            {
                LocalWriteLine($"ContextMenu is exists");
                return;
            }

            if (string.IsNullOrWhiteSpace(_vm.Title))
            {
                LocalWriteLine($"PageTitle is empty");
                _pageCaptionVisibility.SetDelayVisibility(Visibility.Collapsed, 0, NeeView.Windows.Data.DelayValueOverwriteOption.Force);
            }
            else
            {
                LocalWriteLine($"IsMouseOver: {this.LayerStatusArea.IsMouseOver}, {this.DockStatusArea.IsMouseOver}");
                var isVisible = Config.Current.PageTitle.IsEnabled && (this.LayerStatusArea.IsMouseOver || this.DockStatusArea.IsMouseOver);
                if (isVisible)
                {
                    _pageCaptionVisibility.SetDelayVisibility(Visibility.Visible, 0);
                }
                else
                {
                    _pageCaptionVisibility.SetDelayVisibility(Visibility.Collapsed, (int)(Config.Current.AutoHide.AutoHideDelayTime * 1000));
                }
            }
        }

        #endregion ページタイトル管理

        #region IHasDpiScale

        public DpiScale GetDpiScale()
        {
            return _dpiProvider.DpiScale;
        }

        #endregion IHasDpiScale

        #region IHasRenameManager

        public RenameManager GetRenameManager()
        {
            return this.RenameManager;
        }

        #endregion IHasRenameManager

        #region [開発用]

        public MainWindowViewModel ViewModel => _vm;

        // [開発用] 設定初期化
        [Conditional("DEBUG")]
        private static void Debug_Initialize()
        {
            DebugGesture.Initialize();
        }

        [Conditional("LOCAL_DEBUG")]
        private void LocalWriteLine(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(CultureInfo.InvariantCulture, s, args)}");
        }

        #endregion

    }

}
