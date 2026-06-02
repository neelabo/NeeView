//#define LOCAL_DEBUG

using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Windows.Controls;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView.Windows
{
    [LocalDebug]
    public partial class WindowStateManager : ObservableObject
    {
        private readonly Window _window;
        private WindowStateEx _previousState;
        private WindowStateEx _currentState;
        private WindowStateEx _resumeState = WindowStateEx.Normal;
        private WindowStateEx _minimizeResumeState = WindowStateEx.Normal;
        private Rect _restoreBounds;
        private bool _isProgress;
        private WindowPlacement _windowPlacement = new();


        public WindowStateManager(Window window)
        {
            _window = window;
            _window.StateChanged += Window_StateChanged;
            Update();
        }

        public event EventHandler? StateChanged;
        public event EventHandler<WindowStateExChangedEventArgs>? StateEditing;
        public event EventHandler<WindowStateExChangedEventArgs>? StateEdited;


        public WindowStateEx CurrentState
        {
            get => _currentState;
            set => SetWindowState(value);
        }

        public WindowStateEx PreviousState
        {
            get => _previousState;
        }

        /// <summary>
        /// EX状態を解除したときに戻る状態 (Normal or Maximize)
        /// </summary>
        public WindowStateEx ResumeState
        {
            get => _resumeState;
            set => _resumeState = value;
        }

        /// <summary>
        /// 通常ウィンドウの復元論理座標
        /// </summary>
        public Rect RestoreBounds => _restoreBounds;

        public bool IsFullScreen => _currentState == WindowStateEx.FullScreen;

        public bool IsFullDesktop => _currentState == WindowStateEx.FullDesktop;


        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (_isProgress) return;

            Update();
        }

        public void Update()
        {
            switch (_window.WindowState)
            {
                case WindowState.Minimized:
                    ToMinimize();
                    break;
                case WindowState.Normal:
                    ToNormalizeMaybe();
                    break;
                case WindowState.Maximized:
                    ToMaximizeMaybe();
                    break;
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
                case WindowStateEx.FullDesktop:
                    ToFullDesktop();
                    break;
            }
        }

        private void BeginEdit(WindowStateExChangedEventArgs editArgs)
        {
            StateEditing?.Invoke(this, editArgs);
            _isProgress = true;

            switch (_currentState)
            {
                case WindowStateEx.Normal:
                    FromNormalize();
                    break;
                case WindowStateEx.FullDesktop:
                    FromFullDesktop();
                    break;
            }
        }

        private void EndEdit(WindowStateExChangedEventArgs editArgs)
        {
            var newState = editArgs.NewState;
            if (newState != _currentState)
            {
                _previousState = _currentState;
                _currentState = newState;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }

            _isProgress = false;
            StateEdited?.Invoke(this, editArgs);
        }


        public void ToMinimize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.Minimized);
            BeginEdit(editArgs);

            _window.WindowState = WindowState.Minimized;

            EndEdit(editArgs);
        }

        public void ToNormalizeMaybe()
        {
            if (_currentState == WindowStateEx.FullDesktop || (_currentState == WindowStateEx.Minimized && _minimizeResumeState == WindowStateEx.FullDesktop))
            {
                ToFullDesktop();
            }
            else
            {
                ToNormalize();
            }
        }

        public void ToNormalize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.Normal);
            BeginEdit(editArgs);

            _resumeState = WindowStateEx.Normal;
            _minimizeResumeState = WindowStateEx.Normal;

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Normal;

            EndEdit(editArgs);
        }

        private void FromNormalize()
        {
            if (_currentState != WindowStateEx.Normal) return;

            // 通常ウィンドウサイズの保存
            _restoreBounds = _window.RestoreBounds;
            LocalDebug.WriteLine($"RestoreBounds: {_restoreBounds}");

            // 通常ウィンドウの状態を保存
            _windowPlacement = WindowPlacementTools.StoreWindowPlacement(_window, Config.Current.Window.IsRestoreAeroSnapPlacement);
        }

        public void ToMaximizeMaybe()
        {
            if (_currentState == WindowStateEx.FullScreen || (_currentState == WindowStateEx.Minimized && _minimizeResumeState == WindowStateEx.FullScreen))
            {
                ToFullScreen();
            }
            else if (_currentState == WindowStateEx.Minimized && _minimizeResumeState == WindowStateEx.FullDesktop)
            {
                //  直前が Maximized の FullDesktop の最小化状態の復帰で Mazimized で復元されてしまう現象の対処
                ToFullDesktop();
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

            _resumeState = WindowStateEx.Maximized;
            _minimizeResumeState = WindowStateEx.Maximized;

            _window.ResizeMode = ResizeMode.CanResize;
            if (CurrentState == WindowStateEx.FullScreen && Windows11Tools.IsWindows11OrGreater)
            {
                _window.WindowState = WindowState.Normal;
            }
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

            _minimizeResumeState = WindowStateEx.FullScreen;

            // NOTE: Windowsショートカットによる移動ができなくなるので、タブレットに限定する
            if (WindowParameters.IsTabletMode)
            {
                _window.ResizeMode = ResizeMode.CanMinimize;
            }

            if (_window.WindowState == WindowState.Maximized && (PreviousState != WindowStateEx.FullScreen || CurrentState != WindowStateEx.Minimized))
            {
                _window.WindowState = WindowState.Normal;
            }

            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;

            WindowChromePatch.ResetMaximizedWindowSize(_window, true);

            EndEdit(editArgs);
        }


        public void ToFullDesktop()
        {
            // タブレットモードでは無効
            if (WindowParameters.IsTabletMode) return;

            if (_isProgress) return;

            var editArgs = new WindowStateExChangedEventArgs(_currentState, WindowStateEx.FullDesktop);
            BeginEdit(editArgs);

            _minimizeResumeState = WindowStateEx.FullDesktop;

            _window.WindowState = WindowState.Normal;
            _window.WindowStyle = WindowStyle.None;
            _window.ResizeMode = ResizeMode.CanMinimize;

            _window.Left = SystemParameters.VirtualScreenLeft;
            _window.Top = SystemParameters.VirtualScreenTop;
            _window.Width = SystemParameters.VirtualScreenWidth;
            _window.Height = SystemParameters.VirtualScreenHeight;

            EndEdit(editArgs);
        }

        private void FromFullDesktop()
        {
            if (_currentState != WindowStateEx.FullDesktop) return;

            // 通常ウィンドウサイズの復元
            RestoreWindowPlacement(_windowPlacement);
        }

        public void ToggleMinimize()
        {
            if (_currentState != WindowStateEx.Minimized)
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
            if (_currentState != WindowStateEx.Maximized)
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

        public void SetFullDesktop(bool isFullDesktop)
        {
            if (isFullDesktop)
            {
                ToFullDesktop();
            }
            else
            {
                ReleaseFullDesktop();
            }
        }

        public void ToggleFullDesktop()
        {
            if (WindowParameters.IsTabletMode) return;

            if (IsFullDesktop)
            {
                ReleaseFullDesktop();
            }
            else
            {
                ToFullDesktop();
            }
        }

        public void ReleaseFullDesktop()
        {
            if (!IsFullDesktop) return;

            if (_resumeState == WindowStateEx.Maximized || WindowParameters.IsTabletMode)
            {
                ToMaximize();
            }
            else
            {
                ToNormalize();
            }
        }

        /// <summary>
        /// Windowの状態保存
        /// </summary>
        /// <param name="withAeroSnap">エアロスナップを通常サイズとして記憶</param>
        /// <returns></returns>
        public WindowPlacement StoreWindowPlacement(bool withAeroSnap)
        {
            // フルデスクトップの場合は直前の復元情報を返す
            if (_currentState == WindowStateEx.FullDesktop || (_currentState == WindowStateEx.Minimized && _minimizeResumeState == WindowStateEx.FullDesktop))
            {
                return _windowPlacement;
            }

            Debug.Assert(_currentState != WindowStateEx.FullDesktop);
            return WindowPlacementTools.StoreWindowPlacement(_window, withAeroSnap).WithState(_currentState);
        }

        /// <summary>
        /// Windowの状態復元
        /// </summary>
        /// <param name="placement">状態データ</param>
        public void RestoreWindowPlacement(WindowPlacement placement)
        {
            WindowPlacementTools.RestoreWindowPlacement(_window, placement);

            // 特殊状態復元
            switch (placement.WindowStateEx)
            {
                case WindowStateEx.FullScreen:
                    ToFullScreen();
                    break;
                case WindowStateEx.FullDesktop:
                    ToFullDesktop();
                    break;
            }

            // 復元情報更新
            _windowPlacement = placement.WithState(WindowStateEx.Normal);
        }
    }
}
