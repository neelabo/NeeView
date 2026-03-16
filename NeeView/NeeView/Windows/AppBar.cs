using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;


namespace NeeView.Windows
{
    /// <summary>
    /// TaskBar row-level accessor
    /// </summary>
    /// <remarks>
    /// from https://mntone.hateblo.jp/entry/2020/08/02/111309
    /// </remarks>
    public static class AppBar
    {
        // Note: This is constant in every DPI.
        private const int _hideAppBarSpace = 2;


        public static bool IsAutoHideAppBar()
        {
            var appBarData = new APPBARDATA()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
            };

            var result = PInvoke.SHAppBarMessage(PInvoke.ABM_GETSTATE, ref appBarData);
            if (result == PInvoke.ABS_AUTOHIDE)
            {
                return true;
            }

            return false;
        }

        private static nuint GetAutoHideAppBar(uint uEdge, Win32Rect rc)
        {
            var data = new APPBARDATA()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                uEdge = uEdge,
                rc = rc,
            };

            return PInvoke.SHAppBarMessage(PInvoke.ABM_GETAUTOHIDEBAREX, ref data);
        }

        private static bool HasAutoHideAppBar(IntPtr monitor, Win32Rect area, uint targetEdge)
        {
            if (!IsAutoHideAppBar())
            {
                return false;
            }

            var appBar = GetAutoHideAppBar(targetEdge, area);
            if (appBar == 0)
            {
                return false;
            }

            var appBarMonitor = PInvoke.MonitorFromWindow((HWND)appBar, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            if (!monitor.Equals(appBarMonitor))
            {
                return false;
            }

            return true;
        }

        internal static void ApplyAppBarSpace(IntPtr monitor, Win32Rect monitorArea, ref Win32Rect workArea)
        {
            if (HasAutoHideAppBar(monitor, monitorArea, PInvoke.ABE_TOP)) workArea.top += _hideAppBarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, PInvoke.ABE_LEFT)) workArea.left += _hideAppBarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, PInvoke.ABE_RIGHT)) workArea.right -= _hideAppBarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, PInvoke.ABE_BOTTOM)) workArea.bottom -= _hideAppBarSpace;
        }
    }
}
