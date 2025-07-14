using System;
using System.Windows.Data;

namespace NeeView
{
    // 常に false を返すコンバータ
    [ValueConversion(typeof(object), typeof(bool))]
    public class AnyToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}
