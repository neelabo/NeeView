using NeeView.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;

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
        private const string _appbarClass = "Shell_TrayWnd";

        // Note: This is constant in every DPI.
        private const int _hideAppbarSpace = 2;


        public static bool IsAutoHideAppBar()
        {
            var appBarData = APPBARDATA.Create();
            var result = NativeMethods.SHAppBarMessage(AppBarMessages.ABM_GETSTATE, ref appBarData);
            if (result.ToInt32() == (int)AppBarState.ABS_AUTOHIDE)
            {
                return true;
            }

            return false;
        }

        private static IntPtr GetAutoHideAppBar(AppBarEdges uEdge, RECT rc)
        {
            var data = new APPBARDATA()
            {
                cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                uEdge = uEdge,
                rc = rc,
            };
            return NativeMethods.SHAppBarMessage(AppBarMessages.ABM_GETAUTOHIDEBAREX, ref data);
        }

        private static bool HasAutoHideAppBar(IntPtr monitor, RECT area, AppBarEdges targetEdge)
        {
            if (!IsAutoHideAppBar())
            {
                return false;
            }

            var appbar = GetAutoHideAppBar(targetEdge, area);
            if (appbar == IntPtr.Zero)
            {
                return false;
            }

            var appbarMonitor = NativeMethods.MonitorFromWindow(appbar, (int)MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            if (!monitor.Equals(appbarMonitor))
            {
                return false;
            }

            return true;
        }

        public static void ApplyAppbarSpace(IntPtr monitor, RECT monitorArea, ref RECT workArea)
        {
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_TOP)) workArea.top += _hideAppbarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_LEFT)) workArea.left += _hideAppbarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_RIGHT)) workArea.right -= _hideAppbarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_BOTTOM)) workArea.bottom -= _hideAppbarSpace;
        }
    }
}
