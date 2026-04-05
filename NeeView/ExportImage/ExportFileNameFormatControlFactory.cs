using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public static class ExportFileNameFormatControlFactory
    {
        public static ExportFileNameFormatControl CreateOriginalFileNameFormatControl(IExportImageParameter _parameter)
        {
            var propertyName = nameof(_parameter.FileNameFormat0);
            var source = ExportFileNameFormat.CreateDummyFileNameSource(1, 1);

            return CreateFileNameFormatControl(_parameter, propertyName, source, CreteFileNameFormatCheckBinding(_parameter, propertyName, source, new ExportValidateFileNameConverter()));
        }

        public static ExportFileNameFormatControl CreateViewFileNameFormatControl(IExportImageParameter _parameter, int pageCount, int direction)
        {
            var propertyName = pageCount == 1 ? nameof(_parameter.FileNameFormat1) : nameof(_parameter.FileNameFormat2);
            var source = ExportFileNameFormat.CreateDummyFileNameSource(pageCount, direction);

            return CreateFileNameFormatControl(_parameter, propertyName, source, CreteFileNameFormatCheckBinding(_parameter, propertyName, source, new ExportValidateFilePathConverter()));
        }

        private static ExportFileNameFormatControl CreateFileNameFormatControl(IExportImageParameter _parameter, string propertyName, ExportPageSource source, MultiBinding nameFormatCheckBinding)
        {
            return new ExportFileNameFormatControl(_parameter, propertyName, source, nameFormatCheckBinding);
        }

        private static MultiBinding CreteFileNameFormatCheckBinding(IExportImageParameter _parameter, string propertyName, ExportPageSource source, IMultiValueConverter converter)
        {
            var binding = new MultiBinding() { Converter = converter, ConverterParameter = source, };

            binding.Bindings.Add(new Binding(propertyName) { Source = _parameter, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            binding.Bindings.Add(new Binding(nameof(_parameter.Mode)) { Source = _parameter });
            binding.Bindings.Add(new Binding(nameof(_parameter.FileFormat)) { Source = _parameter });

            return binding;
        }
    }
}
