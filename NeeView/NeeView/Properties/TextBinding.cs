using System.Globalization;
using System;
using System.Windows.Data;

namespace NeeView.Properties
{
    public class TextBinding : Binding
    {
        private static readonly ResourceNameToStringConverter _converter = new();

        public TextBinding() : this(null)
        {
        }

        public TextBinding(string? name)
        {
            Source = TextResources.Resource;
            Converter = _converter;
            ConverterParameter = name;
            Mode = BindingMode.OneTime;
        }

        public string? Name
        {
            get { return ConverterParameter as string; }
            set { ConverterParameter = value; }
        }
    }


    public class ResourceNameToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is string name)
            {
                return TextResources.GetString(name);
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
