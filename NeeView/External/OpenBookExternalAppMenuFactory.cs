using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class OpenBookExternalAppMenuFactory : ExternalAppMenuFactory
    {
        public OpenBookExternalAppMenuFactory(ICommandParameterFactory<ExternalApp> parameterFactory) : base(TextResources.GetString("OpenBookExternalAppAsCommand.Menu"), parameterFactory)
        {
            EnabledBinding = new Binding(nameof(BookExternalAppBindingSource.CanOpenExternalApp)) { Source = new BookExternalAppBindingSource() };
        }

        protected override ICommand Command { get; } = new OpenExternalAppCommand();
        protected override Binding? EnabledBinding { get; }

        /// <summary>
        /// 現在ブック用 外部アプリを開くコマンド
        /// </summary>
        private class OpenExternalAppCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not ExternalAppParameter e) return false;
                return BookOperation.Current.BookControl.CanOpenExternalApp(e.ExternalApp);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not ExternalAppParameter e) return;
                BookOperation.Current.BookControl.OpenExternalApp(e.ExternalApp);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        private class BookExternalAppBindingSource : BindableBase
        {
            private readonly DummyExternalApp _dummyExternalApp = new DummyExternalApp();

            public BookExternalAppBindingSource()
            {
                BookOperation.Current.BookChanged += BookOperation_BookChanged;
            }

            public bool CanOpenExternalApp => BookOperation.Current.BookControl.CanOpenExternalApp(_dummyExternalApp);

            private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
            {
                RaisePropertyChanged(nameof(CanOpenExternalApp));
            }
        }

        private class DummyExternalApp : IExternalApp
        {
            public string? Command { get; set; }
            public string Parameter { get; set; } = "";
            public string? WorkingDirectory { get; set; }
            public ArchivePolicy ArchivePolicy { get; set; }
        }

    }

}
