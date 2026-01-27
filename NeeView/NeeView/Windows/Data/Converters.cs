using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Windows.Data
{
    /// <summary>
    /// オブジェクトがNullの場合はVisiitily.Collapsedを返す
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Trueの時にVisibility.Collapsedにする
    /// </summary>
    [System.Windows.Data.ValueConversion(typeof(bool), typeof(Visibility))]
    public class NotBoolToVisiblityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

