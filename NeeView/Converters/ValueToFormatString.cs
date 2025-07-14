using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class ValueToFormatString<T> : IValueConverter
    {
        private readonly Func<T, string> _format;

        public ValueToFormatString(Func<T, string> format)
        {
            _format = format;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T x ? _format.Invoke(x) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
