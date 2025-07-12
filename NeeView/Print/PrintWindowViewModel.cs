using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Documents;

namespace NeeView
{
    public class PrintWindowCloseEventArgs : EventArgs
    {
        public bool? Result { get; set; }
    }


    public class PrintWindowViewModel : BindableBase
    {
        private readonly PrintModel _model;
        private bool _isEnabled = true;
        private FrameworkElement? _mainContent;
        private List<FixedPage> _pageCollection = new();
        private RelayCommand? _resetCommand;
        private RelayCommand? _printCommand;
        private RelayCommand? _cancelCommand;
        private RelayCommand? _printDialogCommand;


        public PrintWindowViewModel(PrintContext context)
        {
            _model = new PrintModel(context);
            _model.Restore(Config.Current.Print);

            _model.PropertyChanged += PrintService_PropertyChanged;
            _model.Margin.PropertyChanged += PrintService_PropertyChanged;

            UpdatePreview();
        }


        public event EventHandler<PrintWindowCloseEventArgs>? Close;


        public PrintModel Model => _model;

        /// <summary>
        /// ウィンドウ操作有効フラグ
        /// </summary>
        /// <remarks>
        /// PrintDialog はサブウィンドウをオーナーとして表示できないため、ウィンドウ操作無効を独自実装するためのフラグ。
        /// 完全ではなく、ALT+F4等のシステムコマンドは実行されてしまうが、それらは許容する。
        /// </remarks>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public FrameworkElement? MainContent
        {
            get { return _mainContent; }
            set { if (_mainContent != value) { _mainContent = value; RaisePropertyChanged(); } }
        }

        public List<FixedPage> PageCollection
        {
            get { return _pageCollection; }
            set { if (_pageCollection != value) { _pageCollection = value; RaisePropertyChanged(); } }
        }

        public RelayCommand ResetCommand
        {
            get { return _resetCommand = _resetCommand ?? new RelayCommand(ResetCommand_Execute); }
        }

        public RelayCommand PrintCommand
        {
            get { return _printCommand = _printCommand ?? new RelayCommand(PrintCommand_Execute); }
        }

        public RelayCommand CancelCommand
        {
            get { return _cancelCommand = _cancelCommand ?? new RelayCommand(CancelCommand_Execute); }
        }

        public RelayCommand PrintDialogCommand
        {
            get { return _printDialogCommand = _printDialogCommand ?? new RelayCommand(PrintDialogCommand_Execute); }
        }


        /// <summary>
        /// 終了処理
        /// </summary>
        public void Closed()
        {
            Config.Current.Print = _model.CreateMemento();
        }

        /// <summary>
        /// パラメータ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdatePreview();
        }

        /// <summary>
        /// プレビュー更新
        /// </summary>
        [MemberNotNull(nameof(PageCollection))]
        private void UpdatePreview()
        {
            PageCollection = _model.CreatePageCollection();
        }

        private void ResetCommand_Execute()
        {
            var dialog = new MessageDialog(TextResources.GetString("PrintResetDialog.Message"), TextResources.GetString("PrintResetDialog.Title"));
            dialog.Commands.Add(UICommands.OK);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(App.Current.MainWindow);
            if (result.IsPossible)
            {
                _model.ResetDialog();
                Config.Current.Print = null;
            }
        }

        private void PrintCommand_Execute()
        {
            _model.Print();
            Close?.Invoke(this, new PrintWindowCloseEventArgs() { Result = true });
        }

        private void CancelCommand_Execute()
        {
            Close?.Invoke(this, new PrintWindowCloseEventArgs() { Result = false });
        }

        private void PrintDialogCommand_Execute()
        {
            if (_model.PrintDialogShown) return;

            try
            {
                IsEnabled = false;
                _model.ShowPrintDialog();
            }
            finally
            {
                IsEnabled = true;
            }
            UpdatePreview();
        }
    }
}
