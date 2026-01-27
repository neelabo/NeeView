using System;
using System.Globalization;

namespace NeeView.Text
{
    public class FormatValue
    {
        private readonly object? _value;
        private readonly string? _format;
        private readonly Func<object, object?>? _converter;

        public FormatValue(object? value) : this(value, null, null)
        {
        }

        public FormatValue(object? value, string? format) : this(value, format, null)
        {
        }

        public FormatValue(object? value, string? format, Func<object, object?>? converter)
        {
            _value = value;
            _format = format;
            _converter = converter;
        }


        public bool IsEmpty => _value is null;


        public string ToFormatString()
        {
            if (_value is null)
            {
                return "";
            }

            try
            {
                var value = _converter != null ? _converter(_value) : _value;
                if (value is null)
                {
                    return "";
                }
                else if (_format != null)
                {
                    return string.Format(CultureInfo.InvariantCulture, _format, value);
                }
                else
                {
                    return value?.ToString() ?? "";
                }
            }
            catch
            {
                return _value?.ToString() ?? "";
            }
        }

        public override string ToString()
        {
            return ToFormatString();
        }

        #region Filters

        public static object? SupportTypeConverter<T>(object value)
        {
            if (value is T target)
            {
                return target;
            }
            return null;
        }

        public static object? NotDefaultValueConverter<T>(object value)
        {
            if (value is T target && !object.Equals(target, default(T)))
            {
                return target;
            }
            return null;
        }

        #endregion Filters
    }
}
