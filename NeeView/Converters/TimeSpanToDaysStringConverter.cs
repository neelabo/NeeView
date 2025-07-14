using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class TimeSpanToDaysStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is TimeSpan timeSpan ? timeSpan.Days.ToString() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return TimeSpan.FromDays(double.Parse((string)value));
            }
            catch
            {
                return value;
            }
        }
    }

}
