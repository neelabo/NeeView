using H.NotifyIcon;
using System.Windows;

namespace NeeView
{
    public static class AppStateTools
    {
        public static void FlushWindowState(Window window)
        {
            if (AppState.Current.IsHideWindow)
            {
                window.Hide(enableEfficiencyMode: false);
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.Show(disableEfficiencyMode: true);
                window.Activate();
            }
        }
    }
}
