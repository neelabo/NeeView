using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using NeeView.Interop;

namespace NeeView.Windows
{
    public static class WindowTools
    {
        [Flags]
        public enum WindowStyle : uint
        {
            None = 0,

            // タイトルバー上にウィンドウメニューボックスを持つウィンドウを作成します。
            SystemMenu = WindowStyles.WS_SYSMENU,

            // 最小化ボタンを持つウィンドウを作成します。 WS_SYSMENU スタイルも指定する必要があります。拡張スタイルに WS_EX_CONTEXTHELP を指定することはできません。
            MinimizeBox = WindowStyles.WS_MINIMIZEBOX,

            MaximizeBox = WindowStyles.WS_MAXIMIZEBOX,
        }

        /// <summary>
        /// ウィンドウスタイルの一部無効化
        /// </summary>
        /// <param name="window"></param>
        /// <param name="disableStyleFlags">無効化するスタイル</param>
        public static void DisableStyle(Window window, WindowStyle disableStyleFlags)
        {
            if (window.IsLoaded)
            {
                UpdateSystemMenu();
            }
            else
            {
                window.SourceInitialized +=
                    (s, e) => UpdateSystemMenu();
            }

            void UpdateSystemMenu()
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                var style = NativeMethods.GetWindowLong(handle, (int)WindowLongFlags.GWL_STYLE);
                style = style & (~(int)disableStyleFlags);
                var result = NativeMethods.SetWindowLong(handle, (int)WindowLongFlags.GWL_STYLE, style);
                if (result == 0)
                {
                    // SetWindowLong failed.
                }
            }
        }

        /// <summary>
        /// システムメニューを表示
        /// </summary>
        /// <param name="window"></param>
        public static void ShowSystemMenu(Window window)
        {
            if (window is null) return;

            var hWnd = (new WindowInteropHelper(window)).Handle;
            if (hWnd == IntPtr.Zero) return;

            var hMenu = NativeMethods.GetSystemMenu(hWnd, false);
            if (hMenu == IntPtr.Zero) return;

            var screenPos = window.PointToScreen(Mouse.GetPosition(window));
            uint command = NativeMethods.TrackPopupMenuEx(hMenu, (uint)(TrackPopupMenuFlags.TPM_LEFTBUTTON | TrackPopupMenuFlags.TPM_RETURNCMD), (int)screenPos.X, (int)screenPos.Y, hWnd, IntPtr.Zero);
            if (command == 0) return;

            NativeMethods.PostMessage(hWnd, (uint)WindowMessages.WM_SYSCOMMAND, new IntPtr(command), IntPtr.Zero);
        }

        /// <summary>
        /// オブジェクトの所属するウィンドウのハンドルを取得する
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static IntPtr GetWindowHandleFromObject(DependencyObject dependencyObject)
        {
            var window = dependencyObject as Window ?? Window.GetWindow(dependencyObject);
            if (window is null) return IntPtr.Zero;

            return new WindowInteropHelper(window).Handle;
        }

        /// <summary>
        /// ウィンドウハンドルを取得
        /// </summary>
        /// <remarks>
        /// もし window に null が渡されても例外を発生させない
        /// </remarks>
        /// <returns></returns>
        public static IntPtr GetWindowHandle(Window window)
        {
            return window is not null ? new WindowInteropHelper(window).Handle : IntPtr.Zero;
        }

        /// <summary>
        /// メインウィンドウのハンドルを取得する
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetWindowHandle()
        {
            return AppDispatcher.Invoke(() => GetWindowHandle(App.Current.MainWindow));
        }

        /// <summary>
        /// Window を非アクティブ状態で復元
        /// </summary>
        /// <param name="window">ウィンドウ</param>
        /// <param name="referenceWindow">基準となるウィンドウ</param>
        public static void RestoreNoActiveWindow(Window window, Window referenceWindow, bool isBehind)
        {
            IntPtr hWnd = GetWindowHandle(window);
            if (hWnd == IntPtr.Zero) return;

            // 非アクティブのまま復元
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOWNOACTIVATE);

            // WindowChrome のアクティブ化を抑制
            NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);

            // referenceWindow の前後に配置 (Z-order 修正)
            IntPtr hWndReference = GetWindowHandle(referenceWindow);
            if (hWndReference == IntPtr.Zero) return;

            SetWindowZOrder(hWnd, hWndReference, isBehind);
        }

        /// <summary>
        /// Window の Z-order 修正
        /// </summary>
        public static void SetWindowZOrder(Window window, Window referenceWindow, bool isFront)
        {
            IntPtr hWnd = GetWindowHandle(window);
            IntPtr hWndReference = GetWindowHandle(referenceWindow);

            SetWindowZOrder(hWnd, hWndReference, isFront);
        }

        /// <summary>
        /// Window の Z-order 修正
        /// </summary>
        private static void SetWindowZOrder(IntPtr hWnd, IntPtr hWndReference, bool isFront)
        {
            if (hWnd == IntPtr.Zero) return;

            // 手前表示は通常ウィンドウの最前面にする
            hWndReference = isFront ? NativeMethods.HWND_TOP : hWndReference;

            NativeMethods.SetWindowPos(hWnd, hWndReference, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
        }

    }
}
