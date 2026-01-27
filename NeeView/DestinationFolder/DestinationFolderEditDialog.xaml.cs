using System.Windows;

namespace NeeView
{
    /// <summary>
    /// DestinationFolderEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class DestinationFolderEditDialog : Window
    {
        private readonly DestinationFolderEditDialogViewModel _vm;

        // for designer
        public DestinationFolderEditDialog() : this(new DestinationFolder())
        {
        }

        public DestinationFolderEditDialog(DestinationFolder model)
        {
            InitializeComponent();

            _vm = new DestinationFolderEditDialogViewModel(model);
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
