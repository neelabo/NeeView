using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    public class ReadOnlyTextBox : TextBox
    {
        public ReadOnlyTextBox()
        {
            this.IsReadOnly = true;
            this.BorderThickness = new Thickness(0);
            this.Background = Brushes.Transparent;
            this.Foreground = (Brush)Application.Current.FindResource("Panel.Foreground");

            InputMethod.SetIsInputMethodEnabled(this, false);

            PreviewMouseLeftButtonDown += ReadOnlyTextBox_PreviewMouseLeftButtonDown;
            GotFocus += ReadOnlyTextBox_GotFocus;
        }

        private void ReadOnlyTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox t && !t.IsFocused)
            {
                t.Focus();
                e.Handled = true;
            }
        }

        private void ReadOnlyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox t)
            {
                t.SelectAll();
            }
        }
    }
}
