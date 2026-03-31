using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ExportBookDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ExportBookDialog : Window
    {
        private readonly ExportBookDialogViewModel? _vm;

        public ExportBookDialog()
        {
            InitializeComponent();
        }

        public ExportBookDialog(ExportBookParameter parameter) : this()
        {
            _vm = new ExportBookDialogViewModel(parameter);
            this.DataContext = _vm;

            this.Loaded += ExportBookDialog_Loaded;
            this.KeyDown += ExportBookDialog_KeyDown;
        }

        private void ExportBookDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.SaveButton.Focus();
        }

        private void ExportBookDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            bool? result = await _vm.ShowSelectSaveFileDialogAsync(this, CancellationToken.None);
            if (result == true)
            {
                this.DialogResult = true;
                this.Close();
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DestinationFolderOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            DestinationFolderDialog.ShowDialog(this);
            _vm.UpdateDestinationFolderList();
        }
    }
}
