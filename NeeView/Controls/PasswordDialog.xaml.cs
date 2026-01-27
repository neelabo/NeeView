using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSelectDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public PasswordDialog()
        {
            InitializeComponent();

            this.OkButton.IsEnabled = false;

            this.InputValueTextBox.InputValueChanged += InputValueTextBox_InputValueChanged;
            this.InputValueTextBox.Decided += InputValueTextBox_Decided;
        }


        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(PasswordDialog), new PropertyMetadata("", MessageProperty_Changed));

        private static void MessageProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordDialog control)
            {
                control.MessageTextBlock.Text = (string?)e.NewValue;
            }
        }


        public string InputValue { get; private set; } = "";


        private void InputValueTextBox_InputValueChanged(object? sender, EventArgs e)
        {
            this.InputValue = this.InputValueTextBox.InputValue;
            this.OkButton.IsEnabled = !string.IsNullOrEmpty(this.InputValue);
        }

        private void InputValueTextBox_Decided(object? sender, EventArgs e)
        {
            this.InputValue = this.InputValueTextBox.InputValue;
            if (string.IsNullOrEmpty(this.InputValue))
            {
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void PasswordDialog_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
