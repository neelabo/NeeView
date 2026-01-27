using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// メインビュー用のページコマンド リソース
    /// </summary>
    public static class MainViewExternalAppTools
    {
        private static readonly OpenExternalAppCommand _openExternalAppCommand = new();
        private static readonly OpenExternalAppDialogCommand _openExternalAppDialogCommand = new();


        public static MenuItem CreateExternalAppItem(ICommandParameterFactory<ExternalApp> parameterFactory)
        {
            var menuItem = ExternalAppCollectionUtility.CreateExternalAppItem(true, _openExternalAppCommand, _openExternalAppDialogCommand);
            menuItem.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(ViewPageBindingSource.AnyViewPages)) { Source = ViewPageBindingSource.Default });
            menuItem.SubmenuOpened += (s, e) => UpdateExternalAppMenu(menuItem.Items, parameterFactory);
            return menuItem;
        }

        public static void UpdateExternalAppMenu(ItemCollection items, ICommandParameterFactory<ExternalApp> parameterFactory)
        {
            ExternalAppCollectionUtility.UpdateExternalAppItems(items, _openExternalAppCommand, _openExternalAppDialogCommand, parameterFactory);
        }


        public static bool IsValidExternalAppIndex(int index)
        {
            return Config.Current.System.ExternalAppCollection.IsValidIndex(index);
        }

        public static bool CanOpenExternalApp(ICommandParameterFactory<ExternalApp> parameterFactory, int index)
        {
            var externalApps = Config.Current.System.ExternalAppCollection;
            if (!externalApps.IsValidIndex(index)) return false;

            return _openExternalAppCommand.CanExecute(parameterFactory.CreateParameter(externalApps[index]));
        }

        public static void OpenExternalApp(ICommandParameterFactory<ExternalApp> parameterFactory, int index)
        {
            var externalApps = Config.Current.System.ExternalAppCollection;
            if (!externalApps.IsValidIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));

            _openExternalAppCommand.Execute(parameterFactory.CreateParameter(externalApps[index]));
        }


        /// <summary>
        /// メインビュー用 外部アプリを開くコマンド
        /// </summary>
        private class OpenExternalAppCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not ExternalAppParameter e) return false;
                return BookOperation.Current.Control.CanOpenApplication(e.ExternalApp, e.Option.MultiPagePolicy);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not ExternalAppParameter e) return;
                BookOperation.Current.Control.OpenApplication(e.ExternalApp, e.Option.MultiPagePolicy);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// メインビュー用 外部アプリの選択メニューを表示するコマンド
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
