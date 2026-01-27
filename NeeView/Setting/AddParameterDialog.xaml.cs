using NeeView.Properties;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// AddParameterDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddParameterDialog : Window
    {
        public AddParameterDialog(ValidationRule? rule)
        {
            InitializeComponent();

            this.AddButton.Content = TextResources.GetString("Word.OK");
            this.CancelButton.Content = TextResources.GetString("Word.Cancel");

            this.Loaded += AddParameterDialog_Loaded;
            this.KeyDown += AddParameterDialog_KeyDown;

            var binding = new Binding(nameof(Input)) { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            if (rule != null)
            {
                binding.ValidationRules.Add(rule);
            }
            this.InputTextBox.SetBinding(TextBox.TextProperty, binding);
        }

        private void AddParameterDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }


        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(AddParameterDialog), new PropertyMetadata(null));


        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register("Input", typeof(string), typeof(AddParameterDialog), new PropertyMetadata(null));


        private void AddParameterDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.InputTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(this.InputTextBox)) return;
            if (string.IsNullOrEmpty(Input.Trim())) return;

            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.DialogResult = true;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.DialogResult = false;
                Close();
                e.Handled = true;
            }
        }
    }
}
