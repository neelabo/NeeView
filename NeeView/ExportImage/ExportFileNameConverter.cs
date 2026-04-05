using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// ExportFileNameFormat をもとに、ファイル名の例を表示するための Converter
    /// </summary>
    public class ExportFileNameConverter : IMultiValueConverter
    {
        private readonly Func<string?, string> _validateFunc;


        public ExportFileNameConverter()
        {
            _validateFunc = s => s ?? "";
        }

        public ExportFileNameConverter(Func<string?, string> validateFunc)
        {
            _validateFunc = validateFunc;
        }


        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var source = (IExportPageSource)parameter;
                var format = GetValue<string>(values, 0);
                var mode = GetValue<ExportImageMode>(values, 1);
                var imageFormat = GetValue<BitmapImageFormat>(values, 2);

                if (string.IsNullOrEmpty(format))
                {
                    return "";
                }
                else
                {
                    var name = ExportFileNameFormat.Format(format, source, 1, mode, imageFormat);
                    var result = "e.g. " + _validateFunc(name);

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


    /// <summary>
    /// 不正なファイル名を修正する ExportFileNameConverter
    /// </summary>
    public class ExportValidateFileNameConverter : ExportFileNameConverter
    {
        public ExportValidateFileNameConverter() : base(LoosePath.ValidFileName)
        {
        }
    }

    /// <summary>
    /// 不正なパス名を修正する ExportFileNameConverter
    /// </summary>
    public class ExportValidateFilePathConverter : ExportFileNameConverter
    {
        public ExportValidateFilePathConverter() : base(LoosePath.ValidPath)
        {
        }
    }
}

