#pragma warning disable CA1822

using NeeView.Windows;
using System.Windows;

namespace NeeView
{
    [WordNodeMember]
    public class WindowAccessor
    {
        private readonly WindowProxy _window;

        public WindowAccessor(WindowProxy window)
        {
            _window = window;
        }

        [WordNodeMember]
        public bool IsOpen
        {
            get
            {
                return AppDispatcher.Invoke(() => _window.Window is not null);
            }
            set
            {
                if (value)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        [WordNodeMember]
        public double Left
        {
            get
            {
                return AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    return window?.Left ?? 0.0;
                });
            }
            set
            {
                AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    if (window is not null)
                    {
                        window.Left = value;
                    }
                });
            }
        }

        [WordNodeMember]
        public double Top
        {
            get
            {
                return AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    return window?.Top ?? 0.0;
                });
            }
            set
            {
                AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    if (window is not null)
                    {
                        window.Top = value;
                    }
                });
            }
        }

        [WordNodeMember]
        public double Width
        {
            get
            {
                return AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    return window?.Width ?? 0.0;
                });
            }
            set
            {
                AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    if (window is not null)
                    {
                        window.Width = value;
                    }
                });
            }
        }

        [WordNodeMember]
        public double Height
        {
            get
            {
                return AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    return window?.Height ?? 0.0;
                });
            }
            set
            {
                AppDispatcher.Invoke(() =>
                {
                    var window = _window.Window;
                    if (window is not null)
                    {
                        window.Height = value;
                    }
                });
            }
        }

        [WordNodeMember(DocumentType = typeof(WindowStateEx))]
        public string State
        {
            get
            {
                return AppDispatcher.Invoke(() =>
                {
                    return _window.GetWindowState().ToString();
                });
            }
            set
            {
                AppDispatcher.Invoke(() =>
                {
                    _window.SetWindowState(value.ToEnum<WindowStateEx>());
                });
            }
        }

        [WordNodeMember]
        public void Open()
        {
            AppDispatcher.Invoke(() => _window.Open());
        }

        [WordNodeMember]
        public void Close()
        {
            AppDispatcher.Invoke(() => _window.Close());
        }

        [WordNodeMember]
        public void Focus()
        {
            AppDispatcher.Invoke(() => _window.Focus());
        }
    }



    public abstract class WindowProxy
    {
        public abstract Window? Window { get; }

        public virtual WindowStateEx GetWindowState()
        {
            return Window?.WindowState.ToWindowStateEx() ?? WindowStateEx.None;
        }

        public virtual void SetWindowState(WindowStateEx value)
        {
            if (value == WindowStateEx.None) return;
            if (Window is null) return;

            if (Window is IHasWindowController controller)
            {
                controller.WindowController.WindowStateManager.SetWindowState(value);
            }
            else
            {
                Window.WindowState = value.ToWindowState();
            }
        }

        public virtual void Open()
        {
            Window?.Focus();
        }

        public virtual void Close()
        {
            Window?.Close();
        }

        public virtual void Focus()
        {
            Window?.Focus();
        }
    }
}
