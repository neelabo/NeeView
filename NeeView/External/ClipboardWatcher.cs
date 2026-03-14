using System;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace NeeView
{
    public class ClipboardWatcher : IDisposable
    {
        private readonly Window _window;
        private nint _handle;
        private bool _disposedValue;

        public ClipboardWatcher(Window window)
        {
            _window = window;
            Open();
        }


        public event EventHandler? ClipboardChanged;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                Close();
                _disposedValue = true;
            }
        }

        ~ClipboardWatcher()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private void Open()
        {
            if (_handle != nint.Zero) throw new InvalidOperationException("ClipboardListener is already opened.");

            _handle = new WindowInteropHelper(_window).Handle;
            if (_handle == nint.Zero) throw new InvalidOperationException("Cannot get window handle.");

            PInvoke.AddClipboardFormatListener((HWND)_handle);

            HwndSource source = HwndSource.FromHwnd(_handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private void Close()
        {
            if (_handle != nint.Zero)
            {
                PInvoke.RemoveClipboardFormatListener((HWND)_handle);
                _handle = nint.Zero;
            }
        }

        private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == PInvoke.WM_CLIPBOARDUPDATE)
            {
                ClipboardChanged?.Invoke(this, EventArgs.Empty);
            }

            return nint.Zero;
        }
    }
}
