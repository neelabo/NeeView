using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(value as string);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
