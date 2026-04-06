using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Text;

namespace NeeView
{
    public class ExportFileNameFormatModel : BindableBase
    {
        private readonly PropertyProxy<ExportImageParameter, string> _property;

        public ExportFileNameFormatModel(ExportImageParameter parameter, PropertyProxy<ExportImageParameter, string> property, ExportPageSource source, string defaultFileNameFormat)
        {
            Parameter = parameter;

            _property = property;

            DefaultFileNameFormat = defaultFileNameFormat;

            Func<string?, string> validateFunc = parameter is ExportBookParameter ? LoosePath.ValidPath : LoosePath.ValidFileName;
            Source = new ExportFileNameConverterParameter(source, validateFunc);

            RuleData = new ExportFileNameRuleData(parameter, source, 1);

            HelpText = GetHelpText();
        }


        public ExportImageParameter Parameter { get; }

        public ExportFileNameConverterParameter Source { get; }

        public ExportFileNameRuleData RuleData { get; }

        public string FileNameFormat
        {
            get
            {
                return _property.GetValue();
            }
            set
            {
                if (_property.GetValue() != value)
                {
                    _property.SetValue(value);
                    RaisePropertyChanged();
                }
            }
        }

        public string DefaultFileNameFormat { get; } = "";

        public string HelpText { get; }


        public RelayCommand LostFocusCommand
        {
            get
            {
                return field ??= new RelayCommand(Execute);

                void Execute()
                {
                    if (string.IsNullOrWhiteSpace(FileNameFormat))
                    {
                        FileNameFormat = DefaultFileNameFormat;
                    }
                }
            }
        }


        private string GetHelpText()
        {
            var sb = new StringBuilder();
            sb.AppendLine(TextResources.GetString("ExportFileNameFormat.Note.Basic"));
            sb.AppendLine("e.g., {Index:000} → 001,002,...");
            sb.AppendLine();
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.BookKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.NameKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.EntryPathKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.PageKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.IndexKey));
            sb.AppendLine();
            sb.AppendLine(TextResources.GetString("ExportFileNameFormat.Note.TwoPages"));
            sb.AppendLine("e.g., {Book}_{Page1}-{Page2}");
            sb.AppendLine();
            sb.AppendLine(GetHelpSuffixText("1"));
            sb.AppendLine(GetHelpSuffixText("2"));
            sb.AppendLine(GetHelpSuffixText("L"));
            sb.Append(GetHelpSuffixText("R"));

            return sb.ToString();
        }

        private string GetHelpWordText(string word)
        {
            return $"- {word} ... {TextResources.GetString("ExportFileNameFormat." + word)}";
        }

        private string GetHelpSuffixText(string suffix)
        {
            return $"- {suffix} ... {TextResources.GetString("ExportFileNameFormat.Suffix" + suffix)}";
        }
    }
}
