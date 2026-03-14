// from https://github.com/takanemu/WPFDragAndDropSample

// WIN32APIの高DPI対応
// from http://grabacr.net/archives/1105

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace NeeView.Windows
{
    /// <summary>
    /// マウスカーソル座標取得(WIN32)
    /// </summary>
    public static class CursorInfo
    {
        public static Int32Point GetNativeCursorPos()
        {
            PInvoke.GetCursorPos(out var pos);
            return pos;
        }

        public static void SetNativeCursorPos(Int32Point pos)
        {
            PInvoke.SetCursorPos(pos.X, pos.Y);
        }

        public static void SetNativeCursorPos(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);
        }

        /// <summary>
        /// 現在のマウスカーソルのクライアント座標を取得
        /// </summary>
        /// <param name="visual">ハンドルを取得するためのビジュアル要素</param>
        /// <returns></returns>
        public static Point GetNowPosition(Visual visual)
        {
            PInvoke.GetCursorPos(out var point);
            return GetClientPosition(point, visual);
        }

        /// <summary>
        /// ネイティブ座標をクライアント座標に変換
        /// </summary>
        /// <param name="point">ネイティブ座標</param>
        /// <param name="visual">ハンドルを取得するためのビジュアル要素</param>
        /// <returns></returns>
        public static Point GetClientPosition(Int32Point point, Visual visual)
        {
            if (HwndSource.FromVisual(visual) is not HwndSource source)
            {
                return new Point(double.NaN, double.NaN);
            }

            var hwnd = source.Handle;
            var isSuccess = PInvoke.ScreenToClient((HWND)hwnd, ref point);
            if (!isSuccess)
            {
                return new Point(double.NaN, double.NaN);
            }

            var dpiScaleFactor = GetDpiScaleFactor(visual);
            return new Point(point.X / dpiScaleFactor.X, point.Y / dpiScaleFactor.Y);
        }

        /// <summary>
        /// ネイティブ座標を UIElement 座標に変換
        /// </summary>
        /// <param name="point">ネイティブ座標</param>
        /// <param name="relativeTo">求める座標系</param>
        /// <returns></returns>
        public static Point GetPosition(Int32Point point, UIElement relativeTo)
        {
            var window = Window.GetWindow(relativeTo);
            if (window is null)
            {
                return new Point(double.NaN, double.NaN);
            }

            var clientPosition = GetClientPosition(point, window);
            return window.TranslatePoint(clientPosition, relativeTo);
        }

        /// <summary>
        /// 現在のマウスカーソルのスクリーン座標を取得
        /// </summary>
        /// <returns></returns>
        public static Point GetNowScreenPosition(Window window)
        {
            PInvoke.GetCursorPos(out var point);

            var dpiScaleFactor = GetDpiScaleFactor(window);
            return new Point(point.X / dpiScaleFactor.X, point.Y / dpiScaleFactor.Y);
        }

        /// <summary>
        /// 現在の <see cref="T:System.Windows.Media.Visual"/> から、DPI 倍率を取得します。
        /// </summary>
        /// <returns>
        /// X 軸 および Y 軸それぞれの DPI 倍率を表す <see cref="T:System.Windows.Point"/>
        /// 構造体。取得に失敗した場合、(1.0, 1.0) を返します。
        /// </returns>
        public static Point GetDpiScaleFactor(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null && source.CompositionTarget != null)
            {
                return new Point(
                    source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
            }

            return new Point(1.0, 1.0);
        }
    }
}
