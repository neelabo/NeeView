using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class ExportFileNameConverter : IMultiValueConverter
    {
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var source = GetValue<ExportFileNameConverterParameter>(values, 0);
                var format = GetValue<string>(values, 1);
                var mode = GetValue<ExportImageMode>(values, 2);
                var imageFormat = GetValue<BitmapImageFormat>(values, 3);

                if (string.IsNullOrEmpty(format))
                {
                    return "";
                }
                else
                {
                    var name = ExportFileNameFormat.Format(format, source, 1, mode, imageFormat);
                    var result = "e.g., " + source.ValidateFunc(name);

                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = $"Error: {ex.Message}";

                return result;
            }
        }

        protected T GetValue<T>(object[] values, int index)
        {
            if (index >= values.Length)
            {
                throw new ArgumentException($"Missing value at index {index}", nameof(values));
            }

            if (values[index] is not T value)
            {
                throw new ArgumentException($"Value at index {index} must be of type {typeof(T).Name}", nameof(values));
            }

            return value;
        }

        protected bool TryGetValue<T>(object[] values, int index, [NotNullWhen(true)] out T? output)
        {
            if (index >= values.Length)
            {
                output = default;
                return false;
            }

            if (values[index] is not T value)
            {
                output = default;
                return false;
            }

            output = value;

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ExportFileNameConverterParameter : IExportPageSource
    {
        public ExportFileNameConverterParameter(IExportPageSource source, Func<string?, string>? validateFunc = null)
        {
            BookAddress = source.BookAddress;
            Direction = source.Direction;
            Elements = source.Elements;
            ValidateFunc = validateFunc ?? (s => s ?? "");
        }

        public string BookAddress { get; }

        public int Direction { get; }

        public List<PageNameElement> Elements { get; }

        public Func<string?, string> ValidateFunc { get; }
    }

}

