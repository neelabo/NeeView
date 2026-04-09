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

        public ExportFileNameFormatModel(ExportImageParameter parameter, PropertyProxy<ExportImageParameter, string> property, ExportPageSource source, string defaultFileNameFormat, string emptyMessage = "")
        {
            Parameter = parameter;

            _property = property;

            DefaultFileNameFormat = defaultFileNameFormat;

            EmptyMessage = emptyMessage;

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
        
        public string EmptyMessage { get; } = "";

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
            sb.AppendLine(Bold(TextResources.GetString("StringFormat.Note.Basic")));
            sb.AppendLine();
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.BookKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.NameKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.EntryPathKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.PartKey) + " (L, R)");
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.PageKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.IndexKey));
            sb.AppendLine();
            sb.AppendLine("e.g., {Index:000} → 012");
            sb.AppendLine();
            sb.AppendLine(Bold(TextResources.GetString("StringFormat.Note.Suffix")));
            sb.AppendLine();
            sb.AppendLine(GetHelpSuffixText("1"));
            sb.AppendLine(GetHelpSuffixText("2"));
            sb.AppendLine(GetHelpSuffixText("L"));
            sb.AppendLine(GetHelpSuffixText("R"));
            sb.AppendLine();
            sb.AppendLine("e.g., {Book}_{Page1}-{Page2}");
            sb.AppendLine();
            sb.AppendLine(Bold(TextResources.GetString("StringFormat.Note.String")));
            sb.AppendLine();
            sb.AppendLine("- / ... " + TextResources.GetString("StringFormat.String.Separator"));
            sb.AppendLine("- # ... " + TextResources.GetString("StringFormat.String.Format"));
            sb.AppendLine();
            sb.AppendLine("e.g., {EntryPath:/__} → Foo__Bar");
            sb.Append("e.g., {Name}{Part:(#)} →  Bar(L)");

            return sb.ToString();
        }

        private static string Bold(string s)
        {
            return "<b>" + s + "</b>";
        }

        private string GetHelpWordText(string word)
        {
            return $"- {word} ... {TextResources.GetString("StringFormat." + word)}";
        }

        private string GetHelpSuffixText(string suffix)
        {
            return $"- {suffix} ... {TextResources.GetString("StringFormat.Suffix." + suffix)}";
        }
    }
}
