using System.Windows;

namespace NeeView.Windows
{
    public static class WindowStateExtensions
    {
        public static WindowStateEx ToWindowStateEx(this WindowState self, bool isFullScreen = false)
        {
            return self switch
            {
                WindowState.Minimized
                    => WindowStateEx.Minimized,
                WindowState.Maximized
                    => isFullScreen ? WindowStateEx.FullScreen : WindowStateEx.Maximized,
                _
                    => WindowStateEx.Normal,
            };
        }
    }
}
