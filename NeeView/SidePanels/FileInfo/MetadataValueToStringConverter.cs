using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class MetadataValueToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MetadataValueTools.ToDisplayString(value);

#if false
            if (value is null) return null;

            return value switch
            {
                IEnumerable<string> strings => string.Join("; ", strings),
                DateTime dateTime => dateTime != default ? dateTime.ToString(Config.Current.Information.DateTimeFormat) : null,
                Enum _ => AliasNameExtensions.GetAliasName(value),
                _ => value.ToString(),
            };
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class MetadataValueTools
    {
        public static string? ToDisplayString(object? value)
        {
            if (value is null) return null;

            return value switch
            {
                IEnumerable<string> strings => string.Join("; ", strings),
                DateTime dateTime => dateTime != default ? dateTime.ToString(Config.Current.Information.DateTimeFormat, CultureInfo.CurrentCulture) : null,
                Enum _ => AliasNameExtensions.GetAliasName(value),
                _ => value.ToString(),
            };
        }
    }

}
