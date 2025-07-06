using System.Windows;

namespace NeeView.Windows
{
    public static class WindowStateExtensions
    {
        public static WindowStateEx ToWindowStateEx(this WindowState self)
        {
            return self switch
            {
                WindowState.Minimized
                    => WindowStateEx.Minimized,
                WindowState.Maximized
                    => WindowStateEx.Maximized,
                _
                    => WindowStateEx.Normal,
            };
        }
    }
}
