using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Color = System.Windows.Media.Color;

namespace NeeView.Windows.Controls
{
    public static class ColorDropper
    {
        /// <summary>
        /// 現在のマウスカーソル位置の画面の色を取得します。
        /// </summary>
        public static Color GetColorUnderCursor()
        {
            if (!PInvoke.GetCursorPos(out var point))
            {
                return Colors.Transparent;
            }

            HDC screenDC = PInvoke.GetDC(HWND.Null);

            try
            {
                uint pixel = PInvoke.GetPixel(screenDC, point.X, point.Y);

                byte r = (byte)(pixel & 0x000000FF);
                byte g = (byte)((pixel & 0x0000FF00) >> 8);
                byte b = (byte)((pixel & 0x00FF0000) >> 16);

                return Color.FromRgb(r, g, b);
            }
            finally
            {
                PInvoke.ReleaseDC(HWND.Null, screenDC);
            }
        }

        /// <summary>
        /// クリックされた場所がアプリ外であるか
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsClickOutsideApp(object sender, MouseButtonEventArgs e)
        {
            var hwndForeground = PInvoke.GetForegroundWindow();

            foreach (Window window in Application.Current.Windows)
            {
                var src = (HwndSource)PresentationSource.FromVisual(window);
                if (src is null)
                {
                    continue;
                }

                var hwnd = (HWND)src.Handle;
                if (hwnd == hwndForeground)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
