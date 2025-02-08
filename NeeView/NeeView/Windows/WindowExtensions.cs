using System.Windows;

namespace NeeView.Windows
{
    public static class WindowExtensions
    {
        public static DpiScale GetDpiScale(this Window window)
        {
            var dpi = new DpiScale(1.0, 1.0);
            if (window is IDpiScaleProvider dpiProvider)
            {
                dpi = dpiProvider.GetDpiScale();
            }
            else
            {
                var source = PresentationSource.FromVisual(window);
                if (source != null)
                {
                    var dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                    var dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                    dpi = new DpiScale(dpiScaleX, dpiScaleY);
                }
            }
            return (dpi.DpiScaleX > 0.0 && dpi.DpiScaleY > 0.0) ? dpi : new DpiScale(1.0, 1.0);
        }
    }
}
