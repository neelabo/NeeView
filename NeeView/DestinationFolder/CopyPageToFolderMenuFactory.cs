using NeeView.Properties;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class CopyPageToFolderMenuFactory : DestinationFolderMenuFactory
    {
        public CopyPageToFolderMenuFactory(ICommandParameterFactory<DestinationFolder> parameterFactory) : base(TextResources.GetString("CopyToFolderAsCommand.Menu"), parameterFactory)
        {
        }

        protected override ICommand Command { get; } = new CopyToFolderCommand();

        protected override Binding? EnabledBinding { get; } = new Binding(nameof(ViewPageBindingSource.AnyViewPages)) { Source = ViewPageBindingSource.Default };


        /// <summary>
        /// メインビュー用 対象フォルダーにコピーするコマンド
        /// </summary>
        private class CopyToFolderCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return false;
                return BookOperation.Current.Control.CanCopyToFolder(e.DestinationFolder, e.Option.MultiPagePolicy);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return;
                BookOperation.Current.Control.CopyToFolder(e.DestinationFolder, e.Option.MultiPagePolicy);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
