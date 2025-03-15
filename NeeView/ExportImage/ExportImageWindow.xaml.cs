using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ExportImageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExportImageWindow : Window
    {
        private readonly ExportImageWindowViewModel? _vm;

        public ExportImageWindow()
        {
            InitializeComponent();
        }

        public ExportImageWindow(ExportImageWindowViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;

            this.Loaded += ExportImageWindow_Loaded;
            this.KeyDown += ExportImageWindow_KeyDown;
        }


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
