using System;
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

            //DebugGesture.Initialize(this);
        }

        public ExportImageDialog(ExportImageDialogViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;
        }


        public string FileName => _vm?.FileName ?? "";


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            _vm?.Dispose();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            
            this.SaveButton.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
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
