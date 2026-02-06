using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class MovePageToFolderMenuFactory : DestinationFolderMenuFactory
    {
        public MovePageToFolderMenuFactory(ICommandParameterFactory<DestinationFolder> parameterFactory, IDestinationFolderOption option) : base(TextResources.GetString("MoveToFolderAsCommand.Menu"), parameterFactory)
        {
            EnabledBinding = new Binding(nameof(MoveableViewPageBindingSource.AnyMoveableViewPages)) { Source = new MoveableViewPageBindingSource(option) };
        }

        protected override ICommand Command { get; } = new MoveToFolderCommand();

        protected override Binding? EnabledBinding { get; }

        /// <summary>
        /// メインビュー用 対象フォルダーに移動するコマンド
        /// </summary>
        private class MoveToFolderCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return false;
                return BookOperation.Current.Control.CanMoveToFolder(e.DestinationFolder, e.Option.MultiPagePolicy);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not DestinationFolderParameter e) return;
                BookOperation.Current.Control.MoveToFolder(e.DestinationFolder, e.Option.MultiPagePolicy);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class MoveableViewPageBindingSource : BindableBase
        {
            private readonly IDestinationFolderOption _option;
            private readonly DestinationFolder _dummyFolder = new DestinationFolder();

            public MoveableViewPageBindingSource(IDestinationFolderOption option)
            {
                _option = option;

                PageFrameBoxPresenter.Current.ViewPageChanged += PageFrameBoxPresenter_ViewPageChanged;
            }

            public bool AnyMoveableViewPages => BookOperation.Current.Control.CanMoveToFolder(_dummyFolder, _option.MultiPagePolicy);

            private void PageFrameBoxPresenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
            {
                RaisePropertyChanged(nameof(AnyMoveableViewPages));
            }
        }
    }
}
