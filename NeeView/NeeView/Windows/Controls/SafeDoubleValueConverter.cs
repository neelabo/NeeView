using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public partial class SafeDoubleValueConverter : IValueConverter
    {
        [GeneratedRegex(@"[+-]?(?:\d+\.?\d*|\.\d+)")]
        private static partial Regex _doubleRegex { get; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;

            if (string.IsNullOrWhiteSpace(s))
            {
                return DependencyProperty.UnsetValue;
            }

            if (double.TryParse(s, out double result))
            {
                return result;
            }

            var match = _doubleRegex.Match(s);
            if (match.Success)
            {
                return double.Parse(match.Value, CultureInfo.InvariantCulture);
            }

            return value;
        }
    }
}
