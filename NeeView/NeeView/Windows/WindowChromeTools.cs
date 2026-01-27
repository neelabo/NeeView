using System.Windows;

namespace NeeView.Windows
{
    /// <summary>
    /// WindowChrome 用
    /// </summary>
    public static class WindowChromeTools
    {
        public static bool GetAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedProperty);
        }

        public static void SetAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedProperty, value);
        }

        public static readonly DependencyProperty AttachedProperty =
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(WindowChromeTools), new PropertyMetadata(false, AttachedPropertyChanged));

        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                SetWindowChromeSource(window);
            }
        }


        public static WindowChromeSource? GetSource(DependencyObject obj)
        {
            return (WindowChromeSource?)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, WindowChromeSource? value)
        {
            obj.SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(WindowChromeSource), typeof(WindowChromeTools), new PropertyMetadata(null));



        public static void SetWindowChromeSource(Window window)
        {
            if (GetSource(window) is not null) return;

            SetSource(window, new WindowChromeSource(window));
        }
    }
}
