using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSelectDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSelectDialog : Window
    {
        private readonly PageSelectDialogViewModel _vm;
        private readonly MouseWheelDelta _mouseWheelDelta = new();

        // for designer
        public PageSelectDialog() : this(new PageSelectDialogModel(5, 1, 10))
        {
        }


        public PageSelectDialog(PageSelectDialogModel model)
        {
            InitializeComponent();

            _vm = new PageSelectDialogViewModel(model);
            _vm.Decided += ViewModel_ChangeResult;
            this.DataContext = _vm;

            this.Loaded += PageSelectDialog_Loaded;
            this.ContentRendered += PageSelectDialog_ContentRendered;
        }

        private void ViewModel_ChangeResult(object? sender, PageSelectDialogDecidedEventArgs e)
        {
            this.DialogResult = e.Result;
            this.Close();
        }

        private void PageSelectDialog_Loaded(object? sender, RoutedEventArgs e)
        {
            this.InputValueTextBox.Focus();
        }
        private void PageSelectDialog_ContentRendered(object? sender, EventArgs e)
        {
            this.InputValueTextBox.SelectAll();
        }

        private void PageSelectDialog_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            _vm.AddValue(-turn);
            this.InputValueTextBox.SelectAll();
        }

        private void PageSelectDialog_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
            }
        }

        private void InputValueTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _vm.DecideCommand.Execute(null);
            }
        }
    }

}
