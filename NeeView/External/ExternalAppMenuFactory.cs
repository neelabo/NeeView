using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public abstract class ExternalAppMenuFactory
    {
        private readonly string _name;
        private readonly ICommandParameterFactory<ExternalApp> _parameterFactory;
        private readonly OpenExternalAppDialogCommand _dialogCommand = new();

        public ExternalAppMenuFactory(string name, ICommandParameterFactory<ExternalApp> parameterFactory)
        {
            _name = name;
            _parameterFactory = parameterFactory;
        }


        protected abstract ICommand Command { get; }
        protected virtual Binding? EnabledBinding { get; }


        public MenuItem CreateFolderMenu()
        {
            var menuItem = ExternalAppCollectionUtility.CreateExternalAppItem(_name, true, Command, _dialogCommand, _parameterFactory);
            if (EnabledBinding != null)
            {
                menuItem.SetBinding(MenuItem.IsEnabledProperty, EnabledBinding);
            }

            menuItem.SubmenuOpened += (s, e) => UpdateFolderMenu(menuItem.Items);
            return menuItem;
        }

        public void UpdateFolderMenu(ItemCollection items)
        {
            ExternalAppCollectionUtility.UpdateExternalAppItems(items, Command, _dialogCommand, _parameterFactory);
        }


        /// <summary>
        /// 外部アプリの選択メニューを表示するコマンド
        /// </summary>
        private class OpenExternalAppDialogCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                var window = MainViewComponent.Current.GetWindow();
                ExternalAppDialog.ShowDialog(window);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
