using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// MainWindowに特化したウィンドウ制御
    /// </summary>
    [INotifyPropertyChanged]
    public partial class MainWindowController : WindowController
    {
        private readonly Window _window;
        private readonly WindowStateManager _manager;
        private bool _autoHideMode;


        public MainWindowController(Window window, WindowStateManager manager) : base(window, manager)
        {
            _window = window;

            _manager = manager;
            _manager.StateChanged += WindowStateManager_StateChanged;

            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.IsTopmost),
                (s, e) => OnPropertyChanged(nameof(IsTopmost)));

            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.State),
                (s, e) => _manager.SetWindowState(Config.Current.Window.State));

            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.IsAutoHideInNormal),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.IsAutoHideInMaximized),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.IsAutoHideInFullScreen),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.SubscribePropertyChanged(nameof(WindowConfig.IsAutoHideInFullDesktop),
                (s, e) => UpdatePanelHideMode());
        }


        public override bool IsTopmost
        {
            get { return Config.Current.Window.IsTopmost; }
            set { Config.Current.Window.IsTopmost = value; }
        }

        public bool AutoHideMode
        {
            get { return _autoHideMode; }
            set { SetProperty(ref _autoHideMode, value); }
        }


        private void WindowStateManager_StateChanged(object? sender, EventArgs e)
        {
            Config.Current.Window.State = _manager.CurrentState;
            UpdatePanelHideMode();

            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnWindowStateChanged, null, CommandOption.None);
        }

        public void UpdatePanelHideMode()
        {
            AutoHideMode = _manager.CurrentState switch
            {
                WindowStateEx.Normal => Config.Current.Window.IsAutoHideInNormal,
                WindowStateEx.Maximized => Config.Current.Window.IsAutoHideInMaximized,
                WindowStateEx.FullScreen => Config.Current.Window.IsAutoHideInFullScreen,
                WindowStateEx.FullDesktop => Config.Current.Window.IsAutoHideInFullDesktop,
                _ => false,
            };
        }

        private void ValidateWindowState()
        {
            if (Config.Current.Window.State != WindowStateEx.None) return;

            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    Config.Current.Window.State = WindowStateEx.Normal;
                    break;
                case WindowState.Minimized:
                    Config.Current.Window.State = WindowStateEx.Minimized;
                    break;
                case WindowState.Maximized:
                    Config.Current.Window.State = WindowStateEx.Maximized;
                    break;
            }
        }

        /// <summary>
        /// 状態を最新にする
        /// </summary>
        public void Refresh()
        {
            ValidateWindowState();
            UpdatePanelHideMode();
            _manager.SetWindowState(Config.Current.Window.State);
            OnPropertyChanged("");
        }
    }
}
