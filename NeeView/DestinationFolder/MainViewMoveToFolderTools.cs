using NeeView.Properties;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// メインビュー用のフォルダーに移動コマンド メニュー
    /// </summary>
    public static class MainViewMoveToFolderTools
    {
        private static readonly MoveToFolderCommand _command = new();
        private static readonly OpenDestinationFolderDialogCommand _dialogCommand = new();

        public static MenuItem CreateMoveToFolderItem(ICommandParameterFactory<DestinationFolder> parameterFactory, MoveableViewPageBindingSource bindingSource)
        {
            var menuItem = DestinationFolderCollectionUtility.CreateDestinationFolderItem(TextResources.GetString("MoveToFolderAsCommand.Menu"), true, _command, _dialogCommand, parameterFactory);
            menuItem.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(MoveableViewPageBindingSource.AnyMoveableViewPages)) { Source = bindingSource });
            menuItem.SubmenuOpened += (s, e) => UpdateMoveToFolderMenu(menuItem.Items, parameterFactory);
            return menuItem;
        }

        public static void UpdateMoveToFolderMenu(ItemCollection items, ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            DestinationFolderCollectionUtility.UpdateDestinationFolderItems(items, _command, _dialogCommand, parameterFactory);
        }


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
