using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class ValueToVisibilityConverter<T> : IValueConverter
    {
        public T? Visible { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T obj && EqualityComparer<T>.Default.Equals(obj, Visible) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
