using NeeView.Windows;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace NeeView
{
    public class ExportFileNameRule : ValidationRule
    {
        public BindingProxy? TargetProxy { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (TargetProxy?.Data is ExportFileNameRuleData data)
                {
                    var format = value as string ?? "";
                    ExportFileNameFormat.Format(format, data.Source, data.Index, data.Parameter.Mode, data.Parameter.FileFormat);
                }
                return ValidationResult.ValidResult;
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
        }
    }

    public class ExportFileNameRuleData
    {
        public ExportFileNameRuleData(IExportImageParameter parameter, IExportPageSource source, int index)
        {
            Parameter = parameter;
            Source = source;
            Index = index;
        }

        public IExportImageParameter Parameter { get; }
        public IExportPageSource Source { get; }
        public int Index { get; } = 1;
    }

}

