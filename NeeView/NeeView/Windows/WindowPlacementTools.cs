//#define LOCAL_DEBUG

// from http://grabacr.net/archives/1585
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;


namespace NeeView.Windows
{
    // TODO: AeroSnap保存ON/OFFフラグ。WindowPlacementOptionフラグ？
    [LocalDebug]
    public static partial class WindowPlacementTools
    {
        public static WindowPlacement StoreWindowPlacement(Window window, bool withAeroSnap)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) throw new InvalidOperationException();

            WINDOWPLACEMENT raw = new();
            PInvoke.GetWindowPlacement((HWND)hwnd, ref raw);
            LocalDebug.WriteLine($"Store: Native.WindowPlacement: {raw}");

            if (withAeroSnap)
            {
                if (raw.showCmd == SHOW_WINDOW_CMD.SW_SHOWNORMAL)
                {
                    try
                    {
                        // AeroSnapの座標保存
                        // NOTE: スナップ状態の復元方法が不明なため、現在のウィンドウサイズを通常ウィンドウサイズとして上書きする。
                        raw.rcNormalPosition = GetAeroPlacement(hwnd);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            var placement = ConvertToWindowPlacement(raw);
            LocalDebug.WriteLine($"Store: Placement={placement}");
            return placement;
        }

        // from http://oldworldgarage.web.fc2.com/programing/tip0006_RestoreWindow.html
        private static Win32Rect GetAeroPlacement(IntPtr hwnd)
        {
            PInvoke.GetWindowRect((HWND)hwnd, out Win32Rect rect);

            // ウィンドウのあるモニターハンドルを取得
            IntPtr hMonitor = PInvoke.MonitorFromRect(rect, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

            // モニター情報取得
            //var monitorInfo = new NativeMethods.MONITORINFOEX();
            //monitorInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
            //monitorInfo.szDeviceName = "";
            //NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo);

            // タスクバーのあるモニターハンドルを取得
            var appBarData = new APPBARDATA();
            appBarData.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            appBarData.hWnd = default;
            PInvoke.SHAppBarMessage(PInvoke.ABM_GETTASKBARPOS, ref appBarData);
            IntPtr hMonitorWithTaskBar = PInvoke.MonitorFromRect(appBarData.rc, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

            // ウィンドウとタスクバーが同じモニターにある？
            if (hMonitor == hMonitorWithTaskBar)
            {
                // 常に表示？
                if (PInvoke.SHAppBarMessage(PInvoke.ABM_GETAUTOHIDEBAR, ref appBarData) == 0)
                {
                    // 座標補正
                    PInvoke.SHAppBarMessage(PInvoke.ABM_GETTASKBARPOS, ref appBarData);
                    switch (appBarData.uEdge)
                    {
                        case PInvoke.ABE_TOP:
                            rect.top = rect.top - (appBarData.rc.bottom - appBarData.rc.top);
                            rect.bottom = rect.bottom - (appBarData.rc.bottom - appBarData.rc.top);
                            break;
                        case PInvoke.ABE_LEFT:
                            rect.left = rect.left - (appBarData.rc.right - appBarData.rc.left);
                            rect.right = rect.right - (appBarData.rc.right - appBarData.rc.left);
                            break;
                    }
                }
            }

            return rect;
        }


        public static void RestoreWindowPlacement(Window window, WindowPlacement placement)
        {
            if (placement == null || !placement.IsValid()) return;
            LocalDebug.WriteLine($"Restore: Placement={placement}");

            var hwnd = new WindowInteropHelper(window).Handle;
            var raw = ConvertToNativeWindowPlacement(placement);
            raw.showCmd = SHOW_WINDOW_CMD.SW_HIDE; // 設定のみ
            // １度目でウィンドウ位置が反映され表示するディスプレイが決定される
            PInvoke.SetWindowPlacement((HWND)hwnd, raw);
            // ２度目でそのディスプレイDPIがウィンドウサイズに反映される
            PInvoke.SetWindowPlacement((HWND)hwnd, raw);

            // WindowState を設定
            window.WindowState = placement.WindowState;
        }

        private static WindowPlacement ConvertToWindowPlacement(WINDOWPLACEMENT raw)
        {
            var memento = new WindowPlacement(
                ConvertToWindowState(raw.showCmd),
                raw.rcNormalPosition.left,
                raw.rcNormalPosition.top,
                raw.rcNormalPosition.Width,
                raw.rcNormalPosition.Height);
            return memento;
        }

        private static WINDOWPLACEMENT ConvertToNativeWindowPlacement(WindowPlacement placement)
        {
            var raw = new WINDOWPLACEMENT();
            raw.length = (uint)Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            raw.flags = 0;
            raw.showCmd = ConvertToNativeShowCmd(placement.WindowState);
            raw.ptMinPosition = new Int32Point(-1, -1);
            raw.ptMaxPosition = new Int32Point(-1, -1);
            raw.rcNormalPosition = new Win32Rect(placement.Left, placement.Top, placement.Right, placement.Bottom);
            return raw;
        }

        private static WindowState ConvertToWindowState(SHOW_WINDOW_CMD showCmd)
        {
            return showCmd switch
            {
                SHOW_WINDOW_CMD.SW_SHOWMINIMIZED => WindowState.Minimized,
                SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED => WindowState.Maximized,
                _ => WindowState.Normal,
            };
        }

        private static SHOW_WINDOW_CMD ConvertToNativeShowCmd(WindowState windowState)
        {
            return windowState switch
            {
                WindowState.Minimized => SHOW_WINDOW_CMD.SW_SHOWMINIMIZED,
                WindowState.Maximized => SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED,
                _ => SHOW_WINDOW_CMD.SW_SHOWNORMAL,
            };
        }

    }

}
