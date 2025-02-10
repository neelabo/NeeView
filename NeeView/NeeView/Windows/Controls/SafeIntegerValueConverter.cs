using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public partial class SafeIntegerValueConverter : IValueConverter
    {
        [GeneratedRegex(@"[+-]?\d+")]
        private static partial Regex _intRegex { get; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;

            if (string.IsNullOrWhiteSpace(s))
            {
                return DependencyProperty.UnsetValue;
            }

            if (int.TryParse(s, out int result))
            {
                return result;
            }

            var match = _intRegex.Match(s);
            if (match.Success)
            {
                return int.Parse(match.Value, CultureInfo.InvariantCulture);
            }

            return value;
        }
    }
}
