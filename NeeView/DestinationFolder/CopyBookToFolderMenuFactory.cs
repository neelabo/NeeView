using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class CopyBookToFolderMenuFactory : DestinationFolderMenuFactory
    {
        public CopyBookToFolderMenuFactory(ICommandParameterFactory<DestinationFolder> parameterFactory) : base(TextResources.GetString("CopyBookToFolderAsCommand.Menu"), parameterFactory)
        {
            EnabledBinding = new Binding(nameof(CopyableViewBookBindingSource.CanCopyBook)) { Source = new CopyableViewBookBindingSource() };
        }


        protected override ICommand Command { get; } = new CopyBookToFolderCommand();

        protected override Binding? EnabledBinding { get; }


        /// <summary>
        /// 現在ブック用 対象フォルダーにコピーするコマンド
        /// </summary>
        private class CopyBookToFolderCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return false;
                return BookOperation.Current.BookControl.CanCopyBookToFolder(e.DestinationFolder);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return;
                BookOperation.Current.BookControl.CopyBookToFolder(e.DestinationFolder);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class CopyableViewBookBindingSource : BindableBase
        {
            private readonly DestinationFolder _dummyFolder = new DestinationFolder();

            public CopyableViewBookBindingSource()
            {
                BookOperation.Current.BookChanged += BookOperation_BookChanged;
            }

            public bool CanCopyBook => BookOperation.Current.BookControl.CanCopyBookToFolder(_dummyFolder);

            private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
            {
                RaisePropertyChanged(nameof(CanCopyBook));
            }
        }

    }
}
