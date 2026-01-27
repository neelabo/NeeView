using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    ///  論理積(OR)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class MultiBooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.OfType<bool>().Any(e => e);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
