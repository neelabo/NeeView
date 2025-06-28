using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    /// <summary>
    /// WinProc をまとめて管理する
    /// </summary>
    public class WindowProcedure
    {
        public delegate IntPtr WindowProcedureFunc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);


        private readonly Window _window;

        private readonly Dictionary<WindowMessages, WindowProcedureFunc> _procedures = new();


        public WindowProcedure(Window window)
        {
            _window = window;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hwnd).AddHook(WndProc);
            }
            else
            {
                _window.Loaded += Window_Loaded;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _window.Loaded -= Window_Loaded;

            var hwnd = new WindowInteropHelper(_window).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        public void Add(WindowMessages msg, WindowProcedureFunc proc)
        {
            // 同じウィンドウメッセージ処理はまだサポートしていない
            if (_procedures.ContainsKey(msg)) throw new ArgumentException("An element with the same window message already exists.", nameof(msg));
            _procedures[msg] = proc;
        }

        public bool Remove(WindowMessages msg)
        {
            return _procedures.Remove(msg);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_procedures.TryGetValue((WindowMessages)msg, out var proc))
            {
                return proc.Invoke(hwnd, msg, wParam, lParam, ref handled);
            }

            return IntPtr.Zero;
        }
    }


    public interface IWindowProcedure
    {
        WindowProcedure WindowProcedure { get; }
    }
}
