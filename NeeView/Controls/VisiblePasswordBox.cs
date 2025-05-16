using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace NeeView
{
    public class VisiblePasswordBox : Control
    {
        static VisiblePasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisiblePasswordBox), new FrameworkPropertyMetadata(typeof(VisiblePasswordBox)));
        }


        private PasswordBox? _passwordBox;
        private TextBox? _textBox;
        private Button? _button;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _passwordBox = GetTemplateChild("PART_InputValuePasswordBox") as PasswordBox ?? throw new InvalidOperationException();
            _textBox = GetTemplateChild("PART_InputValueTextBox") as TextBox ?? throw new InvalidOperationException();
            _button = GetTemplateChild("PART_VisibleToggleButton") as Button ?? throw new InvalidOperationException();

            _passwordBox.KeyDown += InputValuePasswordBox_KeyDown;
            _passwordBox.PasswordChanged += InputValuePasswordBox_PasswordChanged;
            _textBox.KeyDown += InputValueTextBox_KeyDown;
            _textBox.TextChanged += InputValueTextBox_TextChanged;
            _button.Click += VisibleToggleButton_Click;

            UpdateVisibleBox();
        }


        public event EventHandler? Decided;
        public event EventHandler? InputValueChanged;


        public string InputValue
        {
            get { return (string)GetValue(InputValueProperty); }
            private set { SetValue(InputValueProperty, value); }
        }

        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.Register("InputValue", typeof(string), typeof(VisiblePasswordBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, InputValueProperty_Changed));

        private static void InputValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VisiblePasswordBox control)
            {
                control.InputValueChanged?.Invoke(control, EventArgs.Empty);
            }
        }


        public bool IsVisibleValue
        {
            get { return (bool)GetValue(IsVisibleValueProperty); }
            set { SetValue(IsVisibleValueProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleValueProperty =
            DependencyProperty.Register("IsVisibleValue", typeof(bool), typeof(VisiblePasswordBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsVisibleValueProperty_Changed));

        private static void IsVisibleValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VisiblePasswordBox control)
            {
                control.UpdateVisibleBox();
            }
        }


        public ImageSource? VisibleOnIcon
        {
            get { return (ImageSource)GetValue(VisibleOnIconProperty); }
            set { SetValue(VisibleOnIconProperty, value); }
        }

        public static readonly DependencyProperty VisibleOnIconProperty =
            DependencyProperty.Register("VisibleOnIcon", typeof(ImageSource), typeof(VisiblePasswordBox), new PropertyMetadata(null));


        public ImageSource? VisibleOffIcon
        {
            get { return (ImageSource)GetValue(VisibleOffIconProperty); }
            set { SetValue(VisibleOffIconProperty, value); }
        }

        public static readonly DependencyProperty VisibleOffIconProperty =
            DependencyProperty.Register("VisibleOffIcon", typeof(ImageSource), typeof(VisiblePasswordBox), new PropertyMetadata(null));


        private void UpdateVisibleBox()
        {
            if (_textBox == null || _passwordBox == null)
            {
                return;
            }

            if (IsVisibleValue && _textBox.Visibility == Visibility.Visible)
            {
                return;
            }

            if (IsVisibleValue)
            {
                _passwordBox.Visibility = Visibility.Collapsed;
                UpdateInputBoxValue(_passwordBox.Password);
                _textBox.Visibility = Visibility.Visible;
                FocusManager.SetFocusedElement(this, _textBox);
            }
            else
            {
                _textBox.Visibility = Visibility.Collapsed;
                UpdateInputBoxValue(_textBox.Text);
                _passwordBox.Visibility = Visibility.Visible;
                FocusManager.SetFocusedElement(this, _passwordBox);
            }
        }

        private void UpdateInputBoxValue(string inputValue)
        {
            if (_textBox == null || _passwordBox == null)
            {
                return;
            }

            if (_textBox.Text != inputValue)
            {
                _textBox.Text = inputValue;
                _textBox.Select(_textBox.Text.Length, 0);
            }
            if (_passwordBox.Password != inputValue)
            {
                _passwordBox.Password = inputValue;
                _passwordBox.Select(_passwordBox.Password.Length, 0);
            }
        }

        private void InputValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_textBox == null) return;

            if (e.Key == Key.Enter)
            {
                InputValue = _textBox.Text;
                Decided?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void InputValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textBox == null) return;

            this.InputValue = _textBox.Text;
        }

        private void InputValuePasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_passwordBox == null) return;

            if (e.Key == Key.Enter)
            {
                InputValue = _passwordBox.Password;
                Decided?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }
        private void InputValuePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_passwordBox == null) return;

            this.InputValue = _passwordBox.Password;
        }

        private void VisibleToggleButton_Click(object sender, RoutedEventArgs e)
        {
            IsVisibleValue = !IsVisibleValue;
        }
    }


    public static class PasswordBoxExtensions
    {
        public static void Select(this PasswordBox passwordBox, int start, int length)
        {
            // https://stackoverflow.com/questions/990311/how-can-i-set-the-caret-position-to-a-specific-index-in-passwordbox-in-wpf
            passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(passwordBox, [start, length]);
        }
    }
}
