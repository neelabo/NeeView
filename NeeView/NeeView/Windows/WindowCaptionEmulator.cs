using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView.Windows
{
    /// <summary>
    /// ウィンドウキャプションのマウス操作エミュレート
    /// </summary>
    public class WindowCaptionEmulator : IDisposable
    {
        private readonly WindowEx _window;
        private readonly FrameworkElement _target;
        private bool _isDrag;
        private Point _dragStartPoint;
        private bool _disposedValue;

        public WindowCaptionEmulator(Window window, FrameworkElement target)
        {
            _window = new WindowEx(window);
            _target = target;

            _target.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _target.MouseLeftButtonUp += OnMouseLeftButtonUp;
            _target.MouseMove += OnMouseMove;
        }


        public bool IsEnabled { get; set; }

        public bool IsMaximizeEnabled { get; set; } = true;

        public Window Window => _window.Window;


        protected virtual void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            if (!IsEnabled) return;

            if (IsMaximizeEnabled && e.ClickCount == 2 && !WindowParameters.IsTabletMode)
            {
                switch (_window.WindowState)
                {
                    case WindowStateEx.Normal:
                        _window.WindowState = WindowStateEx.Maximized;
                        break;
                    case WindowStateEx.Maximized:
                    case WindowStateEx.FullScreen:
                    case WindowStateEx.FullDesktop:
                        _window.WindowState = WindowStateEx.Normal;
                        break;
                }
                return;
            }

            else if (_window.WindowState == WindowStateEx.Maximized || _window.WindowState.IsExtend)
            {
                _isDrag = true;
                _dragStartPoint = e.GetPosition(_window.Window);
                return;
            }

            WindowDragMove();
        }


        protected virtual void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;

            _isDrag = false;
        }


        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Handled) return;
            if (!_isDrag) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _isDrag = false;
                return;
            }

            var pos = e.GetPosition(_window.Window);
            var dx = Math.Abs(pos.X - _dragStartPoint.X);
            var dy = Math.Abs(pos.Y - _dragStartPoint.Y);
            if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDrag = false;

                double percentHorizontal = e.GetPosition(_window.Window).X / _window.ActualWidth;
                double targetHorizontal = _window.RestoreBounds.Width * percentHorizontal;

                var cursor = Windows.CursorInfo.GetNowScreenPosition(_window.Window);

                var args = new WindowStateChangeEventArgs(_window.Window, _window.WindowState, WindowStateEx.Normal);
                OnWindowStateChange(this, args);
                _window.WindowState = WindowStateEx.Normal;
                _window.Left = cursor.X - targetHorizontal;
                _window.Top = cursor.Y - 8;
                OnWindowStateChanged(this, args);

                WindowDragMove();
            }
        }


        protected virtual void OnWindowStateChange(object sender, WindowStateChangeEventArgs e)
        {
        }

        protected virtual void OnWindowStateChanged(object sender, WindowStateChangeEventArgs e)
        {
        }


        private void WindowDragMove()
        {
            try
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    _window.DragMove();
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _target.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                    _target.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                    _target.MouseMove -= OnMouseMove;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
