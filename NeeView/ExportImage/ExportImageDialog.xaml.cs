using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ExportImageDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ExportImageDialog : Window
    {
        private readonly ExportImageDialogViewModel? _vm;

        public ExportImageDialog()
        {
            InitializeComponent();
        }

        public ExportImageDialog(ExportImageDialogViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;

            this.Loaded += ExportImageWindow_Loaded;
            this.KeyDown += ExportImageWindow_KeyDown;
        }


        public string FileName => _vm?.FileName ?? "";


        private void ExportImageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SaveButton.Focus();
        }

        private void ExportImageWindow_KeyDown(object sender, KeyEventArgs e)
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
