using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class DoubleConstMulConverter : IValueConverter
    {
        public double Coefficient { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * Coefficient;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
