using NeeView.Windows;
using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Window で WindowStateManager を使用している場合は WindowStateManager を優先するラッパークラス
    /// </summary>
    public class WindowEx
    {
        private readonly Window _window;
        private readonly WindowStateManager? _manager;

        public WindowEx(Window window)
        {
            _window = window;

            if (_window is IHasWindowController hasControl)
            {
                _manager = hasControl.WindowController.WindowStateManager;
            }
        }


        public Window Window => _window;


        public event EventHandler? StateChanged
        {
            add
            {
                if (_manager is not null)
                {
                    _manager.StateChanged += value;
                }
                else
                {
                    _window.StateChanged += value;
                }
            }
            remove
            {
                if (_manager is not null)
                {
                    _manager.StateChanged -= value;
                }
                else
                {
                    _window.StateChanged -= value;
                }
            }
        }


        public WindowStateEx WindowState
        {
            get
            {
                if (_manager is not null)
                {
                    return _manager.CurrentState;
                }
                else
                {
                    return _window.WindowState.ToWindowStateEx();
                }
            }
            set
            {
                if (_manager is not null)
                {
                    _manager.CurrentState = value;
                }
                else
                {
                    _window.WindowState = value.ToWindowState();
                }
            }
        }

        public Rect RestoreBounds
        {
            get
            {
                if (_manager is not null)
                {
                    return _manager.RestoreBounds;
                }
                else
                {
                    return _window.RestoreBounds;
                }
            }
        }

        public double Left
        {
            get => _window.Left;
            set => _window.Left = value;
        }

        public double Top
        {
            get => _window.Top;
            set => _window.Top = value;
        }

        public double Width
        {
            get => _window.Width;
            set => _window.Width = value;
        }

        public double Height
        {
            get => _window.Height;
            set => _window.Height = value;
        }

        public double ActualWidth => _window.ActualWidth;

        public double ActualHeight => _window.ActualHeight;


        public static WindowEx? GetWindow(DependencyObject dependencyObject)
        {
            var window = Window.GetWindow(dependencyObject);
            return window is null ? null : new WindowEx(window);
        }

        public void DragMove()
        {
            _window.DragMove();
        }
    }
}
