using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T? True { get; set; }
        public T? False { get; set; }

        public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isTrue && isTrue) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T t && EqualityComparer<T>.Default.Equals(t, True);
        }
    }
}

