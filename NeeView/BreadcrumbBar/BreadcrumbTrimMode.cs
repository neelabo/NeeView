using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

namespace NeeView
{
    public enum BreadcrumbTrimMode
    {
        None = 0,
        Trim,
        Hide,
    }

    public class BreadcrumbTrimModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (BreadcrumbTrimMode)value switch
            {
                BreadcrumbTrimMode.Hide => Visibility.Collapsed,
                _ => Visibility.Visible
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
