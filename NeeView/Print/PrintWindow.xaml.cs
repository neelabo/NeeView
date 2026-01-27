using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PrintWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintWindow : Window
    {
        private readonly PrintWindowViewModel? _vm;


        public PrintWindow()
        {
            InitializeComponent();
        }

        public PrintWindow(PrintContext context) : this()
        {
            _vm = new PrintWindowViewModel(context);
            this.DataContext = _vm;

            _vm.Close += ViewModel_Close;

            this.Loaded += PrintWindow_Loaded;
            this.Closed += PrintWindow_Closed;
            this.PreviewKeyDown += PrintWindow_PreviewKeyDown;
            this.KeyDown += PrintWindow_KeyDown;
        }

        private void PrintWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            this.PrintButton.Focus();
        }

        private void PrintWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            // ウィンドウ無効時のキー入力を無効化する
            if (!_vm.IsEnabled)
            {
                e.Handled = true;
            }
        }

        private void PrintWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void PrintWindow_Closed(object? sender, EventArgs e)
        {
            _vm?.Closed();
        }

        /// <summary>
        /// ウィンドウ終了リクエスト処理
        /// </summary>
        private void ViewModel_Close(object? sender, PrintWindowCloseEventArgs e)
        {
            this.DialogResult = e.Result;
            this.Close();
        }
    }
}
