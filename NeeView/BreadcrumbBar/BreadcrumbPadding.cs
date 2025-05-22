using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace NeeView
{
    public class BreadcrumbPadding : Control
    {
        static BreadcrumbPadding()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbPadding), new FrameworkPropertyMetadata(typeof(BreadcrumbPadding)));
        }

        public BreadcrumbPadding()
        {
            this.MouseLeftButtonDown += BreadcrumbPadding_MouseLeftButtonDown;
            this.GotFocus += BreadcrumbPadding_GotFocus;
        }

        private void BreadcrumbPadding_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is BreadcrumbPadding passing)
            {
                passing.Focus();
            }
        }

        private void BreadcrumbPadding_GotFocus(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetParent(this) is BreadcrumbBar breadcrumbBar)
            {
                breadcrumbBar.RaisePaddingFocused(e);
            }
        }
    }
}
