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
    /// <summary>
    /// VisiblePasswordBox.xaml の相互作用ロジック
    /// </summary>
    public partial class VisiblePasswordBox : UserControl
    {
        public VisiblePasswordBox()
        {
            InitializeComponent();

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


        private void UpdateVisibleBox()
        {
            if (IsVisibleValue && this.InputValueTextBox.Visibility == Visibility.Visible)
            {
                return;
            }

            if (IsVisibleValue)
            {
                this.InputValuePasswordBox.Visibility = Visibility.Collapsed;
                UpdateInputBoxValue(this.InputValuePasswordBox.Password);
                this.InputValueTextBox.Visibility = Visibility.Visible;
                FocusManager.SetFocusedElement(this, InputValueTextBox);
            }
            else
            {
                this.InputValueTextBox.Visibility = Visibility.Collapsed;
                UpdateInputBoxValue(this.InputValueTextBox.Text);
                this.InputValuePasswordBox.Visibility = Visibility.Visible;
                FocusManager.SetFocusedElement(this, InputValuePasswordBox);
            }
        }

        private void UpdateInputBoxValue(string inputValue)
        {
            if (this.InputValueTextBox.Text != inputValue)
            {
                this.InputValueTextBox.Text = inputValue;
                this.InputValueTextBox.Select(InputValueTextBox.Text.Length, 0);
            }
            if (this.InputValuePasswordBox.Password != inputValue)
            {
                this.InputValuePasswordBox.Password = inputValue;
                this.InputValuePasswordBox.Select(InputValuePasswordBox.Password.Length, 0);
            }
        }

        private void InputValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputValue = this.InputValueTextBox.Text;
                Decided?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void InputValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.InputValue = this.InputValueTextBox.Text;
        }

        private void InputValuePasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputValue = this.InputValuePasswordBox.Password;
                Decided?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }
        private void InputValuePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.InputValue = this.InputValuePasswordBox.Password;
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
