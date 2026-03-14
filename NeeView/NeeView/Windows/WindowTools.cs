using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NeeView.Windows
{
    public static class WindowTools
    {
        [Flags]
        public enum WindowStyle : uint
        {
            None = 0,

            // タイトルバー上にウィンドウメニューボックスを持つウィンドウを作成します。
            SystemMenu = WINDOW_STYLE.WS_SYSMENU,

            // 最小化ボタンを持つウィンドウを作成します。 WS_SYSMENU スタイルも指定する必要があります。拡張スタイルに WS_EX_CONTEXTHELP を指定することはできません。
            MinimizeBox = WINDOW_STYLE.WS_MINIMIZEBOX,

            MaximizeBox = WINDOW_STYLE.WS_MAXIMIZEBOX,
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
                var style = PInvoke.GetWindowLong((HWND)handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
                style = style & (~(int)disableStyleFlags);
                var result = PInvoke.SetWindowLong((HWND)handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);
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
        public static unsafe void ShowSystemMenu(Window window)
        {
            if (window is null) return;

            var hWnd = (new WindowInteropHelper(window)).Handle;
            if (hWnd == IntPtr.Zero) return;

            var hMenu = PInvoke.GetSystemMenu((HWND)hWnd, false);
            if (hMenu == IntPtr.Zero) return;

            var screenPos = window.PointToScreen(Mouse.GetPosition(window));
            int command = PInvoke.TrackPopupMenuEx(hMenu, (uint)(TRACK_POPUP_MENU_FLAGS.TPM_LEFTBUTTON | TRACK_POPUP_MENU_FLAGS.TPM_RETURNCMD), (int)screenPos.X, (int)screenPos.Y, (HWND)hWnd, null);
            if (command == 0) return;

            PInvoke.PostMessage((HWND)hWnd, PInvoke.WM_SYSCOMMAND, (nuint)command, 0);
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
        public static IntPtr GetWindowHandle(Window? window)
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
            PInvoke.ShowWindow((HWND)hWnd, SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);

            // WindowChrome のアクティブ化を抑制
            PInvoke.SetWindowPos((HWND)hWnd, default, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);

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
            HWND hWndInsertAfter = isFront ? HWND.HWND_TOP : (HWND)hWndReference;

            PInvoke.SetWindowPos((HWND)hWnd, hWndInsertAfter, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
        }

    }

    /// <summary>
    /// WindowHandle を提供する DI
    /// </summary>
    public interface IHasWindowHandle
    {
        IntPtr GetWindowHandle();
    }
}
