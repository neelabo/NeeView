using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// スライダーと連動することを想定した TextBox
    /// </summary>
    public class SliderValueTextBox : Grid
    {
        private readonly FormattedTextBox _mainTextBox;
        private readonly TextBox _subTextBox;


        public SliderValueTextBox()
        {
            _mainTextBox = new FormattedTextBox();
            _mainTextBox.SetBinding(FormattedTextBox.ValueProperty, new Binding(nameof(Value)) { Source = this, Mode = BindingMode.TwoWay });
            _mainTextBox.SetBinding(FormattedTextBox.FormatProperty, new Binding(nameof(Format)) { Source = this });
            _mainTextBox.SetBinding(FormattedTextBox.ConverterProperty, new Binding(nameof(Converter)) { Source = this });
            _mainTextBox.SetBinding(FormattedTextBox.FormatConverterProperty, new Binding(nameof(FormatConverter)) { Source = this });
            _mainTextBox.ValueDeltaChanged += MainTextBox_ValueDelta;

            _subTextBox = new TextBox();
            _subTextBox.Focusable = false;
            _subTextBox.IsTabStop = false;
            _subTextBox.IsHitTestVisible = false;
            UpdateSubTextBox();
            UpdateTextBoxVisibility();

            this.Children.Add(_mainTextBox);
            this.Children.Add(_subTextBox);
        }


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SliderValueTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string), typeof(SliderValueTextBox), new PropertyMetadata(null, FormatProperty_Changed));

        public IValueConverter Converter
        {
            get { return (IValueConverter)GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }

        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter", typeof(IValueConverter), typeof(SliderValueTextBox), new PropertyMetadata(null, FormatProperty_Changed));

        public IValueConverter FormatConverter
        {
            get { return (IValueConverter)GetValue(FormatConverterProperty); }
            set { SetValue(FormatConverterProperty, value); }
        }

        public static readonly DependencyProperty FormatConverterProperty =
            DependencyProperty.Register("FormatConverter", typeof(IValueConverter), typeof(SliderValueTextBox), new PropertyMetadata(null, FormatProperty_Changed));

        public object SubValue
        {
            get { return (object)GetValue(SubValueProperty); }
            set { SetValue(SubValueProperty, value); }
        }

        public static readonly DependencyProperty SubValueProperty =
            DependencyProperty.Register("SubValue", typeof(object), typeof(SliderValueTextBox), new PropertyMetadata(null));

        public bool IsSubValueEnabled
        {
            get { return (bool)GetValue(IsSubValueEnabledProperty); }
            set { SetValue(IsSubValueEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsSubValueEnabledProperty =
            DependencyProperty.Register("IsSubValueEnabled", typeof(bool), typeof(SliderValueTextBox), new PropertyMetadata(false, IsSubValueEnabledProperty_Changed));

        public IValueDeltaCalculator WheelCalculator
        {
            get { return (IValueDeltaCalculator)GetValue(WheelCalculatorProperty); }
            set { SetValue(WheelCalculatorProperty, value); }
        }

        public static readonly DependencyProperty WheelCalculatorProperty =
            DependencyProperty.Register("WheelCalculator", typeof(IValueDeltaCalculator), typeof(SliderValueTextBox), new PropertyMetadata(null));



        private void MainTextBox_ValueDelta(object? sender, ValueDeltaEventArgs e)
        {
            if (WheelCalculator != null)
            {
                Value = WheelCalculator.Calc(Value, e.Delta);
                _mainTextBox.SelectAll();
            }
        }

        private static void FormatProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderValueTextBox control)
            {
                control.UpdateSubTextBox();
            }
        }

        private static void IsSubValueEnabledProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderValueTextBox control)
            {
                control.UpdateTextBoxVisibility();
            }
        }

        private void UpdateSubTextBox()
        {
            var binding = new Binding(nameof(SubValue)) { Source = this };

            if (this.FormatConverter != null)
            {
                binding.Converter = this.FormatConverter;
            }
            else
            {
                binding.StringFormat = Format;
                binding.Converter = this.Converter;
            }

            _subTextBox.SetBinding(TextBox.TextProperty, binding);
        }

        private void UpdateTextBoxVisibility()
        {
            _mainTextBox.Visibility = IsSubValueEnabled ? Visibility.Hidden : Visibility.Visible;
            _subTextBox.Visibility = IsSubValueEnabled ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
