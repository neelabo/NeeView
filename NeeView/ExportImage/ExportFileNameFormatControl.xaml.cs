using NeeView.Properties;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// ExportFileNameFormatControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ExportFileNameFormatControl : UserControl
    {
        public ExportFileNameFormatControl()
        {
            InitializeComponent();
        }

        public ExportFileNameFormatControl(IExportImageParameter _parameter, string propertyName, ExportPageSource source, MultiBinding nameFormatCheckBinding) : this()
        {
            // format text box
            var textBoxBinding = new Binding(propertyName)
            {
                Source = _parameter,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            textBoxBinding.ValidationRules.Add(new ExportFileNameRule(_parameter, source, 1));
            this.FormatTextBox.SetBinding(TextBox.TextProperty, textBoxBinding);

            // note text block
            var textBlockStyle = new Style(typeof(TextBlock));
            textBlockStyle.Setters.Add(new Setter(TextBlock.TextProperty, nameFormatCheckBinding));
            var hasErrorTrigger = new DataTrigger() { Binding = new Binding("(Validation.HasError)") { Source = this.FormatTextBox }, Value = true, };
            hasErrorTrigger.Setters.Add(new Setter(TextBlock.TextProperty, new Binding("(Validation.Errors)/ErrorContent") { Source = this.FormatTextBox }));
            textBlockStyle.Triggers.Add(hasErrorTrigger);
            this.NoteTextBlock.Style = textBlockStyle;

            // help text
            this.HelpTextBlock.Source = GetHelpText();

            this.IsVisibleChanged += ExportFileNameFormatControl_IsVisibleChanged;
        }

        private void ExportFileNameFormatControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.HelpButton.IsChecked = false;
            }
        }

        private string GetHelpText()
        {
            var sb = new StringBuilder();
            sb.AppendLine(TextResources.GetString("ExportFileNameFormat.Note.Basic"));
            sb.AppendLine("e.g. {Index:000} → 001,002,...");
            sb.AppendLine();
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.BookKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.NameKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.EntryPathKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.PageKey));
            sb.AppendLine(GetHelpWordText(ExportFileNameFormat.IndexKey));
            sb.AppendLine();
            sb.AppendLine(TextResources.GetString("ExportFileNameFormat.Note.TwoPages"));
            sb.AppendLine("e.g. {Book}_{Page1}-{Page2}");
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
