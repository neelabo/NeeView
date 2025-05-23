﻿//#define LOCAL_DEBUG

// from http://grabacr.net/archives/1585
using NeeLaboratory.Generators;
using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

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

            NativeMethods.GetWindowPlacement(hwnd, out WINDOWPLACEMENT raw);
            LocalDebug.WriteLine($"Store: Native.WindowPlacement: {raw}");

            if (withAeroSnap)
            {
                if (raw.ShowCmd == ShowWindowCommands.SW_SHOWNORMAL)
                {
                    try
                    {
                        // AeroSnapの座標保存
                        // NOTE: スナップ状態の復元方法が不明なため、現在のウィンドウサイズを通常ウィンドウサイズとして上書きする。
                        raw.NormalPosition = GetAeroPlacement(hwnd);
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
        private static RECT GetAeroPlacement(IntPtr hwnd)
        {
            NativeMethods.GetWindowRect(hwnd, out RECT rect);

            // ウィンドウのあるモニターハンドルを取得
            IntPtr hMonitor = NativeMethods.MonitorFromRect(ref rect, (uint)MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);

            // モニター情報取得
            //var monitorInfo = new NativeMethods.MONITORINFOEX();
            //monitorInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
            //monitorInfo.szDeviceName = "";
            //NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo);

            // タスクバーのあるモニターハンドルを取得
            var appBarData = new APPBARDATA();
            appBarData.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            appBarData.hWnd = IntPtr.Zero;
            NativeMethods.SHAppBarMessage(AppBarMessages.ABM_GETTASKBARPOS, ref appBarData);
            IntPtr hMonitorWithTaskBar = NativeMethods.MonitorFromRect(ref appBarData.rc, (uint)MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);

            // ウィンドウとタスクバーが同じモニターにある？
            if (hMonitor == hMonitorWithTaskBar)
            {
                // 常に表示？
                if (NativeMethods.SHAppBarMessage(AppBarMessages.ABM_GETAUTOHIDEBAR, ref appBarData) == IntPtr.Zero)
                {
                    // 座標補正
                    NativeMethods.SHAppBarMessage(AppBarMessages.ABM_GETTASKBARPOS, ref appBarData);
                    switch (appBarData.uEdge)
                    {
                        case AppBarEdges.ABE_TOP:
                            rect.top = rect.top - (appBarData.rc.bottom - appBarData.rc.top);
                            rect.bottom = rect.bottom - (appBarData.rc.bottom - appBarData.rc.top);
                            break;
                        case AppBarEdges.ABE_LEFT:
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
            raw.ShowCmd = ShowWindowCommands.SW_HIDE; // 設定のみ
            // １度目でウィンドウ位置が反映され表示するディスプレイが決定される
            NativeMethods.SetWindowPlacement(hwnd, ref raw);
            // ２度目でそのディスプレイDPIがウィンドウサイズに反映される
            NativeMethods.SetWindowPlacement(hwnd, ref raw);

            // WindowState を設定
            window.WindowState = placement.WindowState;
        }

        private static WindowPlacement ConvertToWindowPlacement(WINDOWPLACEMENT raw)
        {
            var memento = new WindowPlacement(
                ConvertToWindowState(raw.ShowCmd),
                raw.NormalPosition.left,
                raw.NormalPosition.top,
                raw.NormalPosition.Width,
                raw.NormalPosition.Height);
            return memento;
        }

        private static WINDOWPLACEMENT ConvertToNativeWindowPlacement(WindowPlacement placement)
        {
            var raw = new WINDOWPLACEMENT();
            raw.Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            raw.Flags = 0;
            raw.ShowCmd = ConvertToNativeShowCmd(placement.WindowState);
            raw.MinPosition = new POINT(-1, -1);
            raw.MaxPosition = new POINT(-1, -1);
            raw.NormalPosition = new RECT(placement.Left, placement.Top, placement.Right, placement.Bottom);
            return raw;
        }

        private static WindowState ConvertToWindowState(ShowWindowCommands showCmd)
        {
            return showCmd switch
            {
                ShowWindowCommands.SW_SHOWMINIMIZED => WindowState.Minimized,
                ShowWindowCommands.SW_SHOWMAXIMIZED => WindowState.Maximized,
                _ => WindowState.Normal,
            };
        }

        private static ShowWindowCommands ConvertToNativeShowCmd(WindowState windowState)
        {
            return windowState switch
            {
                WindowState.Minimized => ShowWindowCommands.SW_SHOWMINIMIZED,
                WindowState.Maximized => ShowWindowCommands.SW_SHOWMAXIMIZED,
                _ => ShowWindowCommands.SW_SHOWNORMAL,
            };
        }

    }

}
