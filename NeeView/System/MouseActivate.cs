//#define LOCAL_DEBUG

using NeeView.Interop;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスによるウィンドウアクティブ化の監視
    /// </summary>
    public class MouseActivate : IDisposable
    {
        private readonly Window _window;
        private readonly WindowProcedure _proc;
        private bool _disposedValue;

        public MouseActivate(Window window)
        {
            if (window is not IWindowProcedure proc) throw new ArgumentException("IWindowProcedure is required.", nameof(window));

            _window = window;
            _window.MouseDown += Window_MouseDown;

            _proc = proc.WindowProcedure;
            _proc.Add(WindowMessages.WM_MOUSEACTIVATE, WindowMessage_MouseActivate);

            MouseActivateService.Current.Add(this);
        }


        public bool IsMouseActivate { get; private set; }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _proc.Remove(WindowMessages.WM_MOUSEACTIVATE);
                    MouseActivateService.Current.Remove(this);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IntPtr WindowMessage_MouseActivate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (Config.Current.Window.MouseActivateAndEat)
            {
                SetMouseActivate();
            }

            return IntPtr.Zero;
        }

        public void SetMouseActivate()
        {
            if (!_window.IsActive)
            {
                IsMouseActivate = true;
                _ = ResetMouseActivateAsync(100);
            }
        }

        private async ValueTask ResetMouseActivateAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
            IsMouseActivate = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseActivate = false;
        }
    }
}
