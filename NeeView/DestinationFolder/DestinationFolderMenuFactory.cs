using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public abstract class DestinationFolderMenuFactory
    {
        private readonly string _name;
        private readonly ICommandParameterFactory<DestinationFolder> _parameterFactory;
        private readonly OpenDestinationFolderDialogCommand _dialogCommand = new();


        public DestinationFolderMenuFactory(string name, ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            _name = name;
            _parameterFactory = parameterFactory;
        }


        protected abstract ICommand Command { get; }
        protected virtual Binding? EnabledBinding { get; }


        public MenuItem CreateFolderMenu()
        {
            var menuItem = DestinationFolderCollectionUtility.CreateDestinationFolderItem(_name, true, Command, _dialogCommand, _parameterFactory);
            if (EnabledBinding != null)
            {
                menuItem.SetBinding(MenuItem.IsEnabledProperty, EnabledBinding);
            }

            menuItem.SubmenuOpened += (s, e) => UpdateFolderMenu(menuItem.Items);
            return menuItem;
        }


        public void UpdateFolderMenu(ItemCollection items)
        {
            DestinationFolderCollectionUtility.UpdateDestinationFolderItems(items, Command, _dialogCommand, _parameterFactory);
        }


        /// <summary>
        /// 対象フォルダーの編集ダイアログを表示するコマンド
        /// </summary>
        private class OpenDestinationFolderDialogCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                var window = MainViewComponent.Current.GetWindow();
                DestinationFolderDialog.ShowDialog(window);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
