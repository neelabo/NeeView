using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class ConditionVisibilityConverter<T> : IValueConverter
    {
        public Func<T, bool> Condition { get; set; } = _ => true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T obj)
            {
                return Condition(obj) ? Visibility.Visible : Visibility.Collapsed;   
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
