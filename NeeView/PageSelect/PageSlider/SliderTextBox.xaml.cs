using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using NeeLaboratory;
using NeeLaboratory.Generators;

namespace NeeView
{
    /// <summary>
    /// SliderTextBox.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SliderTextBox : UserControl, INotifyPropertyChanged
    {
        private readonly MouseWheelDelta _mouseWheelDelta = new();
        private string? _dispText;
        

        public SliderTextBox()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.TextBlock.SizeChanged += TextBlock_SizeChanged;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

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
            get { return _dispText; }
            set { if (_dispText != value) { _dispText = value; RaisePropertyChanged(); } }
        }



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
            this.TextBlock.MinWidth = width;
            this.TextBox.Width = width;
        }


        private void UpdateValue()
        {
            this.DisplayText = $"{Value + 1} / {Maximum + 1}";
        }


        private void SliderTextBox_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            this.TextBox.Visibility = Visibility.Visible;
            this.TextBox.Focus();
        }

        private void SliderTextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            this.TextBox.Visibility = Visibility.Visible;
            this.TextBox.Focus();
        }

        private void SliderTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            UpdateSource();
            this.TextBox.Visibility = Visibility.Hidden;
        }

        private void TextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            this.TextBox.SelectAll();
        }

        private void TextBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            switch (e.Key)
            {
                case Key.Escape:
                    MainWindowModel.Current.FocusMainView();
                    e.Handled = true;
                    break;

                case Key.Return:
                    UpdateSource();
                    this.TextBox.SelectAll();
                    e.Handled = true;
                    break;
            }
        }

        private void UpdateSource()
        {
            this.TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TextBox_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn != 0)
            {
                this.Value = this.Value - turn;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                this.TextBox.SelectAll();
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
