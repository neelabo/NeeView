using System;
using System.Globalization;
using System.Windows.Controls;

namespace NeeView
{
    public class ExportFileNameRule : ValidationRule
    {
        private IExportImageParameter _parameter;
        private IExportPageSource _source;
        private int _index = 1;

        public ExportFileNameRule(IExportImageParameter parameter, IExportPageSource source, int index)
        {
            _parameter = parameter;
            _source = source;
            _index = index;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var format = value as string ?? "";
                ExportFileNameFormat.Format(format, _source, _index, _parameter.Mode, _parameter.FileFormat);
                return ValidationResult.ValidResult;
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
        }
    }
}

