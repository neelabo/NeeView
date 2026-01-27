using NeeView.Windows;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MainWindowCaptionEmulator : WindowCaptionEmulator
    {
        public MainWindowCaptionEmulator(Window window, FrameworkElement target) : base(window, target)
        {
        }


        public WindowStateManager? WindowStateManager { get; set; }


        protected override void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Config.Current.Window.IsCaptionEmulateInFullScreen && this.WindowStateManager?.IsFullScreen == true) return;

            base.OnMouseLeftButtonDown(sender, e);
        }

        protected override void OnWindowStateChange(object sender, WindowStateChangeEventArgs e)
        {
            base.OnWindowStateChange(sender, e);
        }
    }

}
