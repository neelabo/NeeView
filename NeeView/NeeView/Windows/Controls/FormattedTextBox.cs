using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// 入力していないときはフォーマット表示する TextBox
    /// </summary>
    public class FormattedTextBox : EnterTriggerTextBox
    {
        public FormattedTextBox()
        {
            this.IsKeyboardFocusedChanged += EnterTriggerTextBox_IsKeyboardFocusedChanged;

            UpdateBinding();
        }


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(FormattedTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string), typeof(FormattedTextBox), new PropertyMetadata(null, FormatProperty_Changed));

        // 入力時のコンバーター
        public IValueConverter Converter
        {
            get { return (IValueConverter)GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }

        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter", typeof(IValueConverter), typeof(FormattedTextBox), new PropertyMetadata(null, FormatProperty_Changed));

        // 表示時のコンバーター
        public IValueConverter FormatConverter
        {
            get { return (IValueConverter)GetValue(FormatConverterProperty); }
            set { SetValue(FormatConverterProperty, value); }
        }

        public static readonly DependencyProperty FormatConverterProperty =
            DependencyProperty.Register("FormatConverter", typeof(IValueConverter), typeof(FormattedTextBox), new PropertyMetadata(null, FormatProperty_Changed));


        private static void FormatProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FormattedTextBox control)
            {
                control.UpdateBinding();
            }
        }

        private void EnterTriggerTextBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsKeyboardFocused)
            {
                UpdateBinding();
                Application.Current.Dispatcher.BeginInvoke((Action)(() => this.SelectAll()));
            }
            else
            {
                UpdateSource();
                UpdateBinding();
            }
        }

        private void UpdateBinding()
        {
            var isFormat = !this.IsKeyboardFocused && !Validation.GetHasError(this);

            var binding = new Binding(nameof(Value)) { Source = this, Mode = BindingMode.TwoWay };

            if (isFormat)
            {
                if (this.FormatConverter != null)
                {
                    binding.Converter = this.FormatConverter;
                }
                else
                {
                    binding.StringFormat = Format;
                    binding.Converter = this.Converter;
                }
            }
            else
            {
                binding.Converter = this.Converter;
            }

            this.SetBinding(TextBox.TextProperty, binding);
        }

        private void UpdateSource()
        {
            var expression = BindingOperations.GetBindingExpression(this, TextBox.TextProperty);
            expression?.UpdateSource();
        }

    }
}
