using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Shell;

namespace NeeView.Windows
{

    [NotifyPropertyChanged]
    public partial class WindowStateManager : INotifyPropertyChanged
    {
        private readonly Window _window;
        private WindowStateEx _previousState;
        private WindowStateEx _currentState;
        private WindowStateEx _resumeState;
        private bool _isFullScreenMode;
        private bool _isFullScreen;
        private bool _isProgress;


        public WindowStateManager(Window window)
        {
            _window = window;
            _window.StateChanged += Window_StateChanged;
            Update();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<WindowStateExChangedEventArgs>? StateChanged;
        public event EventHandler<WindowStateExChangedEventArgs>? StateEditing;
        public event EventHandler<WindowStateExChangedEventArgs>? StateEdited;


        public WindowState WindowState => _window.WindowState;
        public WindowStateEx CurrentState => _currentState;
        public WindowStateEx PreviousState => _previousState;
        public WindowStateEx ResumeState
        {
            get => _resumeState;
            set => _resumeState = value;
        }
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            private set { SetProperty(ref _isFullScreen, value); }
        }

        public double MaximizedChromeBorderThickness { get; set; } = 8.0;


        private void UpdateIsFullScreen()
        {
            IsFullScreen = _isFullScreenMode && _window.WindowState == WindowState.Maximized;
        }

        private void SetFullScreenMode(bool isEnabled)
        {
            _isFullScreenMode = isEnabled;
            UpdateIsFullScreen();
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (_isProgress) return;

            Update();
        }

        public void Update()
        {
            UpdateIsFullScreen();

            switch (_window.WindowState)
            {
                case WindowState.Minimized:
                    ToMinimize();
                    break;
                case WindowState.Normal:
                    ToNormalize();
                    break;
                case WindowState.Maximized:
                    ToMaximizeMaybe();
                    break;
            }
        }

        public WindowStateEx GetWindowState()
        {
            if (IsFullScreen)
            {
                return WindowStateEx.FullScreen;
            }
            else
            {
                return _window.WindowState switch
                {
                    WindowState.Minimized => WindowStateEx.Minimized,
                    WindowState.Maximized => WindowStateEx.Maximized,
                    WindowState.Normal => WindowStateEx.Normal,
                    _ => throw new NotSupportedException(),
                };
            }
        }

        public void SetWindowState(WindowStateEx state)
        {
            if (_isProgress) return;

            switch (state)
            {
                default:
                case WindowStateEx.Normal:
                    ToNormalize();
                    break;
                case WindowStateEx.Minimized:
                    ToMinimize();
                    break;
                case WindowStateEx.Maximized:
                    ToMaximize();
                    break;
                case WindowStateEx.FullScreen:
                    ToFullScreen();
                    break;
            }
        }

        private void BeginEdit(WindowStateExChangedEventArgs editArgs)
        {
            StateEditing?.Invoke(this, editArgs);
            _isProgress = true;
        }

        private void EndEdit(WindowStateExChangedEventArgs editArgs)
        {
            var nowState = GetWindowState();
            if (nowState != _currentState)
            {
                _previousState = _currentState;
                _currentState = nowState;
                StateChanged?.Invoke(this, new WindowStateExChangedEventArgs(_previousState, _currentState));
            }

            _isProgress = false;
            StateEdited?.Invoke(this, editArgs);
        }


        public void ToMinimize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.Minimized);
            BeginEdit(editArgs);

            //_window.ResizeMode = ResizeMode.CanResize;
            //_window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Minimized;

            EndEdit(editArgs);
        }

        public void ToNormalize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.Normal);
            BeginEdit(editArgs);

            SetFullScreenMode(false);
            _resumeState = WindowStateEx.Normal;

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Normal;

            if (_currentState == WindowStateEx.FullScreen || _currentState == WindowStateEx.Maximized)
            {
                Windows7Tools.RecoveryTaskBar(_window);
            }

            EndEdit(editArgs);
        }

        public void ToMaximizeMaybe()
        {
            if (_isFullScreenMode)
            {
                ToFullScreen();
            }
            else
            {
                ToMaximize();
            }
        }

        public void ToMaximize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.Maximized);
            BeginEdit(editArgs);

            SetFullScreenMode(false);
            _resumeState = WindowStateEx.Maximized;

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Maximized;

            WindowChromePatch.ResetMaximizedWindowSize(_window, false);

            EndEdit(editArgs);
        }

        public void ToFullScreen()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.FullScreen);
            BeginEdit(editArgs);

            // NOTE: Windowsショートカットによる移動ができなくなるので、Windows7とタブレットに限定する
            if (Windows7Tools.IsWindows7 || WindowParameters.IsTabletMode)
            {
                _window.ResizeMode = ResizeMode.CanMinimize;
            }

            if (_window.WindowState == WindowState.Maximized && (PreviousState != WindowStateEx.FullScreen || CurrentState != WindowStateEx.Minimized))
            {
                _window.WindowState = WindowState.Normal;
            }

            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;

            SetFullScreenMode(true);

            WindowChromePatch.ResetMaximizedWindowSize(_window, true);

            EndEdit(editArgs);
        }

        public void ToggleMinimize()
        {
            if (_window.WindowState != WindowState.Minimized)
            {
                SystemCommands.MinimizeWindow(_window);
            }
            else
            {
                SystemCommands.RestoreWindow(_window);
            }
        }

        public void ToggleMaximize()
        {
            if (_window.WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(_window);
            }
            else
            {
                SystemCommands.RestoreWindow(_window);
            }
        }


        public void ToggleFullScreen()
        {
            if (IsFullScreen)
            {
                ReleaseFullScreen();
            }
            else
            {
                ToFullScreen();
            }
        }

        public void ReleaseFullScreen()
        {
            if (!IsFullScreen) return;

            if (_resumeState == WindowStateEx.Maximized || WindowParameters.IsTabletMode)
            {
                ToMaximize();
            }
            else
            {
                ToNormalize();
            }
        }

        public void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                ToFullScreen();
            }
            else
            {
                ReleaseFullScreen();
            }
        }

        /// <summary>
        /// Windowの状態保存
        /// </summary>
        /// <param name="withAeroSnap">エアロスナップを通常サイズとして記憶</param>
        /// <returns></returns>
        public WindowPlacement StoreWindowPlacement(bool withAeroSnap)
        {
            return WindowPlacementTools.StoreWindowPlacement(_window, withAeroSnap).WithIsFullScreen(IsFullScreen);
        }

        /// <summary>
        /// Windowの状態復元
        /// </summary>
        /// <param name="placement">状態データ</param>
        public void RestoreWindowPlacement(WindowPlacement placement)
        {
            if (placement.IsFullScreen)
            {
                ToFullScreen();
            }

            WindowPlacementTools.RestoreWindowPlacement(_window, placement);
        }
    }
}
