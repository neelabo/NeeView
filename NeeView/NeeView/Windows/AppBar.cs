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
        private const string _appBarClass = "Shell_TrayWnd";

        // Note: This is constant in every DPI.
        private const int _hideAppBarSpace = 2;


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

            var appBar = GetAutoHideAppBar(targetEdge, area);
            if (appBar == IntPtr.Zero)
            {
                return false;
            }

            var appBarMonitor = NativeMethods.MonitorFromWindow(appBar, (int)MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            if (!monitor.Equals(appBarMonitor))
            {
                return false;
            }

            return true;
        }

        public static void ApplyAppBarSpace(IntPtr monitor, RECT monitorArea, ref RECT workArea)
        {
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_TOP)) workArea.top += _hideAppBarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_LEFT)) workArea.left += _hideAppBarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_RIGHT)) workArea.right -= _hideAppBarSpace;
            if (HasAutoHideAppBar(monitor, monitorArea, AppBarEdges.ABE_BOTTOM)) workArea.bottom -= _hideAppBarSpace;
        }
    }
}
