using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// bool 反転
    /// </summary>
    public class BooleanReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
