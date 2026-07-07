using System;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class DialogIconToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MessageDialogIcon icon)
            {
                return icon switch
                {
                    MessageDialogIcon.None => Visibility.Collapsed,
                    _ => Visibility.Visible
                };
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
