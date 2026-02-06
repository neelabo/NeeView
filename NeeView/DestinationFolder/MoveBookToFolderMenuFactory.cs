using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class MoveBookToFolderMenuFactory : DestinationFolderMenuFactory
    {
        public MoveBookToFolderMenuFactory(ICommandParameterFactory<DestinationFolder> parameterFactory, IDestinationFolderOption option) : base(TextResources.GetString("MoveBookToFolderAsCommand.Menu"), parameterFactory)
        {
            EnabledBinding = new Binding(nameof(MoveableViewBookBindingSource.CanMoveBook)) { Source = new MoveableViewBookBindingSource() };
        }

        protected override ICommand Command { get; } = new MoveBookToFolderCommand();

        protected override Binding? EnabledBinding { get; }

        /// <summary>
        /// 現在ブック用 対象フォルダーに移動するコマンド
        /// </summary>
        private class MoveBookToFolderCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return false;
                return BookOperation.Current.BookControl.CanMoveBookToFolder(e.DestinationFolder);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return;
                BookOperation.Current.BookControl.MoveBookToFolder(e.DestinationFolder);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class MoveableViewBookBindingSource : BindableBase
        {
            private readonly DestinationFolder _dummyFolder = new DestinationFolder();

            public MoveableViewBookBindingSource()
            {
                BookOperation.Current.BookChanged += BookOperation_BookChanged;
            }

            public bool CanMoveBook => BookOperation.Current.BookControl.CanMoveBookToFolder(_dummyFolder);

            private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
            {
                RaisePropertyChanged(nameof(CanMoveBook));
            }
        }
    }
}
