using System.Windows;

namespace NeeView.Windows
{
    public static class WindowStateExExtensions
    {
        extension(WindowStateEx self)
        {
            public bool IsExtend => self == WindowStateEx.FullScreen || self == WindowStateEx.FullDesktop;

            public WindowState ToWindowState()
            {
                return self switch
                {
                    WindowStateEx.Minimized
                        => WindowState.Minimized,
                    WindowStateEx.Maximized or WindowStateEx.FullScreen
                        => WindowState.Maximized,
                    _
                        => WindowState.Normal,
                };
            }
        }
    }
}
