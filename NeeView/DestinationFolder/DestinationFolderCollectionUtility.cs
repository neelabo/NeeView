using NeeView.Properties;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public static class DestinationFolderCollectionUtility
    {
        private static readonly DestinationFolderCommandParameterFactory _defaultCommandParameterFactory = new();

        /// <summary>
        /// 「フォルダーにコピー」「フォルダーに移動」メニュー作成
        /// </summary>
        /// <param name="title">メニュータイトル</param>
        /// <param name="isEnabled">メニューの有効/無効</param>
        /// <param name="command">実行コマンド</param>
        /// <param name="dialogCommand">設定コマンド</param>
        public static MenuItem CreateDestinationFolderItem(string title, bool isEnabled, ICommand command, ICommand dialogCommand)
        {
            return CreateDestinationFolderItem(title, isEnabled, command, dialogCommand, _defaultCommandParameterFactory);
        }

        public static MenuItem CreateDestinationFolderItem(string title, bool isEnabled, ICommand command, ICommand dialogCommand, ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            var subItem = new MenuItem() { Header = title, IsEnabled = isEnabled };
            UpdateDestinationFolderItems(subItem.Items, command, dialogCommand, parameterFactory);
            return subItem;
        }

        public static void UpdateDestinationFolderItems(ItemCollection items, ICommand command, ICommand dialogCommand, ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            items.Clear();
            if (Config.Current.System.DestinationFolderCollection.Any())
            {
                for (int i = 0; i < Config.Current.System.DestinationFolderCollection.Count; ++i)
                {
                    var folder = Config.Current.System.DestinationFolderCollection[i];
                    var header = MenuItemTools.IntegerToAccessKey(i + 1) + " " + MenuItemTools.EscapeMenuItemString(folder.Name);
                    var parameter = parameterFactory.CreateParameter(folder);
                    items.Add(new MenuItem() { Header = header, ToolTip = folder.Path, Command = command, CommandParameter = parameter });
                }
            }
            else
            {
                items.Add(new MenuItem() { Header = TextResources.GetString("Word.ItemNone"), IsEnabled = false });
            }

            items.Add(new Separator());
            items.Add(new MenuItem() { Header = TextResources.GetString("BookshelfItem.Menu.DestinationFolderOption"), Command = dialogCommand });
        }
    }


    public class DestinationFolderCommandParameterFactory : ICommandParameterFactory<DestinationFolder>
    {
        public object CreateParameter(DestinationFolder folder)
        {
            return folder;
        }
    }
}
