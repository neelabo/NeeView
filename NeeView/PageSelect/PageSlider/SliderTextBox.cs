using NeeLaboratory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public partial class SliderTextBox : Control
    {
        static SliderTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderTextBox), new FrameworkPropertyMetadata(typeof(SliderTextBox)));
        }


        private readonly MouseWheelDelta _mouseWheelDelta = new();
        private TextBlock? _textBlock;
        private TextBox? _textBox;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var root = GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock ?? throw new InvalidOperationException();
            _textBox = GetTemplateChild("PART_TextBox") as TextBox ?? throw new InvalidOperationException();

            root.MouseLeftButtonDown += SliderTextBox_MouseLeftButtonDown;
            root.GotFocus += SliderTextBox_GotFocus;

            _textBlock.SizeChanged += TextBlock_SizeChanged;

            _textBox.MouseWheel += TextBox_MouseWheel;
            _textBox.KeyDown += TextBox_PreviewKeyDown;
            _textBox.GotFocus += TextBox_GotFocus;
            _textBox.LostFocus += TextBox_LostFocus;
        }


        public event EventHandler? ValueChanged;


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SliderTextBox), new PropertyMetadata(0.0));


        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SliderTextBox), new PropertyMetadata(1.0, MaximumPropertyChanged));

        private static void MaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SliderTextBox)?.Update();
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SliderTextBox), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValuePropertyChanged, CoerceValueProperty));

        private static object CoerceValueProperty(DependencyObject d, object baseValue)
        {
            if (d is SliderTextBox control)
            {
                return MathUtility.Clamp((double)baseValue, control.Minimum, control.Maximum);
            }

            return baseValue;
        }

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SliderTextBox)?.UpdateValue();
        }


        public string? DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register("DisplayText", typeof(string), typeof(SliderTextBox), new PropertyMetadata(null));


        private void TextBlock_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            var control = (FrameworkElement?)sender;
            if (control is null) return;

            if (e.WidthChanged && e.NewSize.Width > control.MinWidth)
            {
                SetWidth(e.NewSize.Width);
            }
        }

        private void Update()
        {
            int length = (int)Math.Log10(this.Maximum) + 1;
            var width = (length * 2 + 1) * 7 + 20;
            SetWidth(width);

            UpdateValue();
        }

        private void SetWidth(double width)
        {
            if (_textBlock is null) return;
            if (_textBox is null) return;

            _textBlock.MinWidth = width;
            _textBox.Width = width;
        }


        private void UpdateValue()
        {
            this.DisplayText = $"{Value + 1} / {Maximum + 1}";
        }


        private void SliderTextBox_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (_textBox is null) return;

            _textBox.Visibility = Visibility.Visible;
            _textBox.Focus();
        }

        private void SliderTextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (_textBox is null) return;

            _textBox.Visibility = Visibility.Visible;
            _textBox.Focus();
        }

        private void SliderTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (_textBox is null) return;

            UpdateSource();
            _textBox.Visibility = Visibility.Hidden;
        }

        private void TextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (_textBox is null) return;

            _textBox.SelectAll();
        }

        private void TextBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (_textBox is null) return;
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            switch (e.Key)
            {
                case Key.Escape:
                    MainWindowModel.Current.FocusMainView();
                    e.Handled = true;
                    break;

                case Key.Return:
                    UpdateSource();
                    _textBox.SelectAll();
                    e.Handled = true;
                    break;
            }
        }

        private void UpdateSource()
        {
            if (_textBox is null) return;

            _textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TextBox_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (_textBox is null) return;

            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn != 0)
            {
                this.Value = this.Value - turn;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                _textBox.SelectAll();
            }
            e.Handled = true;
        }
    }

    public class SliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse((string)value, out double result))
            {
                if (result > int.MaxValue)
                {
                    result = int.MaxValue;
                }
                else if (result < 1)
                {
                    result = 1;
                }

                return result - 1;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

}
