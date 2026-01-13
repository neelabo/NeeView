using NeeView.Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class MainViewManager
    {
        private static MainViewManager? _current;
        public static MainViewManager Current => _current ?? throw new InvalidOperationException();


        private MainViewWindow? _window;
        private readonly MainViewComponent _viewComponent;
        private readonly MainView _mainView;
        private readonly MainViewBay _mainViewBay;
        private IDisposableContent? _alternativeContent;
        private readonly ContentControl _defaultSocket;
        private bool _isStoreEnabled = true;
        private readonly MainViewLockerMediator _mediator;
        private readonly MainViewLocker _dockingLocker;
        private MainViewLocker? _floatingLocker;
        private FocusMemento? _focusMemento;


        public static void Initialize(MainViewComponent viewComponent, ContentControl defaultSocket)
        {
            if (_current != null) throw new InvalidOperationException();
            _current = new MainViewManager(viewComponent, defaultSocket);
        }

        public MainViewManager(MainViewComponent viewComponent, ContentControl defaultSocket)
        {
            _viewComponent = viewComponent;

            _mainView = _viewComponent.MainView;
            _defaultSocket = defaultSocket;
            _mainViewBay = new MainViewBay();

            _defaultSocket.Content = _mainView;

            BookOperation.Current.BookChanging += BookOperation_BookChanging;
            BookOperation.Current.BookChanged += BookOperation_BookChanged;

            Config.Current.MainView.AddPropertyChanged(nameof(MainViewConfig.IsFloating), (s, e) => Update(true));

            Config.Current.MainView.SubscribePropertyChanged(nameof(MainViewConfig.AlternativeContent), (s, e) => UpdateAlternativeContent());

            _mediator = new MainViewLockerMediator(_mainView);
            _dockingLocker = new MainViewLocker(_mediator, MainWindow.Current);
            _dockingLocker.Activate();
        }


        public MainViewWindow? Window => _window;
        public MainView MainView => _mainView;
        public MainViewBay MainViewBay => _mainViewBay;


        private void BookOperation_BookChanging(object? sender, BookChangingEventArgs e)
        {
            _mainView.MouseInput?.Cancel();
            _mainViewBay.MouseInput.Cancel();
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            if (_window is null) return;

            if (e.Book is not null)
            {
                RecoveryFloating();
            }
            else if (Config.Current.MainView.IsAutoHide)
            {
                HideFloating();
            }
        }

        public bool IsFloating()
        {
            return Config.Current.MainView.IsFloating;
        }

        public void SetFloating(bool isFloating)
        {
            if (!_isStoreEnabled) return;

            Config.Current.MainView.IsFloating = isFloating;
        }

        public bool RecoveryFloating()
        {
            if (_window is null) return false;

            var config = Config.Current.MainView;

            // ウィンドウが最小化されていたら復元する
            if (_window.WindowState == WindowState.Minimized)
            {
                if (!config.IsAutoShow) return false;

                if (_window.Topmost)
                {
                    _window.WindowState = WindowState.Normal;
                }
                else
                {
                    WindowTools.RestoreNoActiveWindow(_window, App.Current.MainWindow, config.IsFrontAsPossible);
                }
                return true;
            }
            
            // なるべく手前に表示
            if (config.IsFrontAsPossible)
            {
                if (!_window.Topmost)
                {
                    WindowTools.SetWindowZOrder(_window, App.Current.MainWindow, true);
                }
            }

            return false;
        }

        public void HideFloating()
        {
            if (_window is not null)
            {
                if (_window.WindowState != WindowState.Minimized)
                {
                    _window.WindowState = WindowState.Minimized;
                }
            }
        }

        public void Update(bool storeAlternativePanelSource)
        {
            if (Config.Current.MainView.IsFloating)
            {
                Floating(storeAlternativePanelSource);
            }
            else
            {
                Docking();
            }
        }

        private void Floating(bool storeAlternativePanelSource)
        {
            if (_window != null)
            {
                _window.Focus();
                return;
            }

            if (!Config.Current.MainView.WindowPlacement.IsValid())
            {
                var point = _mainView.PointToScreen(new Point(0.0, 0.0));
                Config.Current.MainView.WindowPlacement = new WindowPlacement(WindowState.Normal, (int)point.X + 32, (int)point.Y + 32, (int)_mainView.ActualWidth, (int)_mainView.ActualHeight);
            }

            _dockingLocker.Deactivate();

            _alternativeContent = CreateAlternativeContent(storeAlternativePanelSource);
            _defaultSocket.Content = _alternativeContent.Content;

            InfoMessage.Current.ClearMessage(ShowMessageStyle.Normal);

            _window = new MainViewWindow();
            _window.MainViewSocket.Content = _mainView;

            _window.Closing += (s, e) => Store();
            _window.Closed += (s, e) => SetFloating(false);

            _window.Show();
            _window.Activate();

            if (Config.Current.MainView.IsAutoStretch)
            {
                _mainView.AutoStretchWindow();
            }

            _floatingLocker = new MainViewLocker(_mediator, _window);
            _floatingLocker.Activate();
        }

        private void Docking()
        {
            if (_window is null) return;

            if (_floatingLocker != null)
            {
                _floatingLocker.Deactivate();
                _floatingLocker.Dispose();
                _floatingLocker = null;
            }

            _window.Close();
            _window.Content = null;
            _window = null;

            _alternativeContent?.Dispose();
            _alternativeContent = null;

            // NOTE: コンテンツの差し替えでLoadedイベントが呼ばれないことがあるため、新規コントロールをはさむことで確実にLoadedイベントが呼ばれるようにする。
            _defaultSocket.Content = new ContentControl() { Content = _mainView, IsTabStop = false, Focusable = false };

            _dockingLocker.Activate();
        }

        private IDisposableContent CreateAlternativeContent(bool storeAlternativePanelSource)
        {
            switch (Config.Current.MainView.AlternativeContent)
            {
                case AlternativeContent.Blank:
                    return new NormalAlternativeContent(_mainViewBay);
                case AlternativeContent.PageList:
                    return new LayoutPanelAlternativeContent(nameof(PageListPanel), storeAlternativePanelSource);
                default:
                    throw new NotSupportedException();
            }
        }

        private void UpdateAlternativeContent()
        {
            if (_window is null) return;

            _alternativeContent?.Dispose();
            _alternativeContent = CreateAlternativeContent(true);
            _defaultSocket.Content = _alternativeContent.Content;
        }


        public void SetIsStoreEnabled(bool allow)
        {
            _isStoreEnabled = allow;
        }

        public void Store()
        {
            if (!_isStoreEnabled) return;

            if (_window != null)
            {
                try
                {
                    Config.Current.MainView.WindowPlacement = _window.StoreWindowPlacement();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// メインビューにフォーカスを移す。コマンド用
        /// </summary>
        public void FocusMainView(FocusMainViewCommandParameter parameter)
        {
            if (parameter.IsToggle)
            {
                if (_mainView.IsKeyboardFocusWithin)
                {
                    _focusMemento?.RestoreFocus();
                }
                else
                {
                    UpdatePanels(parameter.NeedClosePanels);
                    _focusMemento = FocusMemento.Create();
                    _mainView.FocusMainView();
                }
            }
            else
            {
                UpdatePanels(parameter.NeedClosePanels);
                _focusMemento = null;
                _mainView.FocusMainView();
            }

            void UpdatePanels(bool closePanels)
            {
                if (closePanels && !IsFloating())
                {
                    CustomLayoutPanelManager.Current.LeftDock.SelectedItem = null;
                    CustomLayoutPanelManager.Current.RightDock.SelectedItem = null;
                }
            }
        }
    }
}
