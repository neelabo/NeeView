using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// メインビュー用のフォルダーにコピーコマンド メニュー
    /// </summary>
    public static class MainViewCopyToFolderTools
    {
        private static readonly CopyToFolderCommand _command = new();
        private static readonly OpenDestinationFolderDialogCommand _dialogCommand = new();


        public static MenuItem CreateCopyToFolderItem(ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            var menuItem = DestinationFolderCollectionUtility.CreateDestinationFolderItem(TextResources.GetString("CopyToFolderAsCommand.Menu"), true, _command, _dialogCommand, parameterFactory);
            menuItem.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(ViewPageBindingSource.AnyViewPages)) { Source = ViewPageBindingSource.Default });
            menuItem.SubmenuOpened += (s, e) => UpdateCopyToFolderMenu(menuItem.Items, parameterFactory);
            return menuItem;
        }

        public static void UpdateCopyToFolderMenu(ItemCollection items, ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            DestinationFolderCollectionUtility.UpdateDestinationFolderItems(items, _command, _dialogCommand, parameterFactory);
        }


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
