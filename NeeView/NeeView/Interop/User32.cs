using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32")]
        internal static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        [DllImport("user32", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        internal static extern uint TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT pt);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        internal static extern bool ScreenToClient(IntPtr hwnd, ref POINT pt);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        internal const int SW_HIDE = 0;
        internal const int SW_SHOWNORMAL = 1;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_SHOWNOACTIVATE = 4;
        internal const int SW_SHOW = 5;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_SHOWMINNOACTIVE = 7;
        internal const int SW_SHOWNA = 8;
        internal const int SW_RESTORE = 9;
        internal const int SW_SHOWDEFAULT = 10;
        internal const int SW_FORCEMINIMIZE = 11;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int VK_LBUTTON = 0x01;
        public const int VK_RBUTTON = 0x02;
        public const int VK_MBUTTON = 0x04;
        public const int VK_XBUTTON1 = 0x05;
        public const int VK_XBUTTON2 = 0x06;

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int code);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const uint SWP_ASYNCWINDOWPOS = 0x4000;
        public const uint SWP_DEFERERASE = 0x2000;
        public const uint SWP_DRAWFRAME = 0x0020;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOREPOSITION = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_SHOWWINDOW = 0x0040;

        public const int HWND_TOP = 0;
        public const int HWND_BOTTOM = 1;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int GetDoubleClickTime();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal extern static void AddClipboardFormatListener(nint hwnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal extern static void RemoveClipboardFormatListener(nint hwnd);
    }
}
