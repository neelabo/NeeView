using System.Windows;
using System.Windows.Controls;

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

            this.IsVisibleChanged += ExportFileNameFormatControl_IsVisibleChanged;
        }

        private void ExportFileNameFormatControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.HelpButton.IsChecked = false;
            }
        }

        private void PopupCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.HelpButton.IsChecked = false;
        }
    }
}
