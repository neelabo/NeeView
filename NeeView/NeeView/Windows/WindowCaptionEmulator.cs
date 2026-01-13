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
        private readonly Window _window;
        private readonly FrameworkElement _target;
        private bool _isDrag;
        private Point _dragStartPoint;
        private bool _disposedValue;

        public WindowCaptionEmulator(Window window, FrameworkElement target)
        {
            _window = window;
            _target = target;

            _target.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _target.MouseLeftButtonUp += OnMouseLeftButtonUp;
            _target.MouseMove += OnMouseMove;
        }


        public bool IsEnabled { get; set; }

        public bool IsMaximizeEnabled { get; set; } = true;

        public Window Window => _window;


        protected virtual void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            if (!IsEnabled) return;

            if (IsMaximizeEnabled && e.ClickCount == 2 && !WindowParameters.IsTabletMode)
            {
                switch (_window.WindowState)
                {
                    case WindowState.Normal:
                        _window.WindowState = WindowState.Maximized;
                        break;
                    case WindowState.Maximized:
                        _window.WindowState = WindowState.Normal;
                        break;
                }
                return;
            }

            else if (_window.WindowState == WindowState.Maximized)
            {
                _isDrag = true;
                _dragStartPoint = e.GetPosition(_window);
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

            var pos = e.GetPosition(_window);
            var dx = Math.Abs(pos.X - _dragStartPoint.X);
            var dy = Math.Abs(pos.Y - _dragStartPoint.Y);
            if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDrag = false;

                double percentHorizontal = e.GetPosition(_window).X / _window.ActualWidth;
                double targetHorizontal = _window.RestoreBounds.Width * percentHorizontal;

                var cursor = Windows.CursorInfo.GetNowScreenPosition(_window);
                _window.Left = cursor.X - targetHorizontal;
                _window.Top = cursor.Y - 8;

                var args = new WindowStateChangeEventArgs(_window, _window.WindowState, WindowState.Normal);
                OnWindowStateChange(this, args);
                _window.WindowState = WindowState.Normal;
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
