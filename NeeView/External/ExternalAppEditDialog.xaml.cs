using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ExternalAppEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ExternalAppEditDialog : Window
    {
        private readonly ExternalAppEditDialogViewModel? _vm;

        public ExternalAppEditDialog()
        {
            InitializeComponent();
        }

        public ExternalAppEditDialog(ExternalApp model) : this()
        {
            _vm = new ExternalAppEditDialogViewModel(model);
            this.DataContext = _vm;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
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
