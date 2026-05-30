using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public static class HoverPreview
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached(
                "Content",
                typeof(object),
                typeof(HoverPreview),
                new PropertyMetadata(null, OnContentChanged));

        public static void SetContent(DependencyObject obj, object value)
            => obj.SetValue(ContentProperty, value);

        public static object GetContent(DependencyObject obj)
            => obj.GetValue(ContentProperty);

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(HoverPreview),
                new PropertyMetadata(true, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);


        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseEnter -= OnMouseEnter;
                element.MouseLeave -= OnMouseLeave;

                if (e.NewValue != null)
                {
                    element.MouseEnter += OnMouseEnter;
                    element.MouseLeave += OnMouseLeave;
                }
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var element = (UIElement)sender;

            var content = GetIsEnabled(element) ? GetContent(element) : null;
            HoverPreviewService.Current.Show(element, content);
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            HoverPreviewService.Current.Hide();
        }
    }

}