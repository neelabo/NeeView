using NeeView.Properties;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public static class ExternalAppCollectionUtility
    {
        private static readonly ExternalAppCommandParameterFactory _defaultCommandParameterFactory = new();

        /// <summary>
        /// 「外部アプリで開く」メニュー作成
        /// </summary>
        /// <param name="isEnabled">メニューの有効/無効</param>
        /// <param name="command">実行コマンド</param>
        /// <param name="dialogCommand">設定コマンド</param>
        public static MenuItem CreateExternalAppItem(bool isEnabled, ICommand command, ICommand dialogCommand)
        {
            return CreateExternalAppItem(isEnabled, command, dialogCommand, _defaultCommandParameterFactory);
        }

        public static MenuItem CreateExternalAppItem(bool isEnabled, ICommand command, ICommand dialogCommand, ICommandParameterFactory<ExternalApp> parameterFactory)
        {
            var subItem = new MenuItem() { Header = TextResources.GetString("OpenExternalAppAsCommand.Menu"), IsEnabled = isEnabled };
            UpdateExternalAppItems(subItem.Items, command, dialogCommand, parameterFactory);
            return subItem;
        }

        public static void UpdateExternalAppItems(ItemCollection items, ICommand command, ICommand dialogCommand, ICommandParameterFactory<ExternalApp> parameterFactory)
        {
            items.Clear();

            if (Config.Current.System.ExternalAppCollection.Any())
            {
                for (int i = 0; i < Config.Current.System.ExternalAppCollection.Count; ++i)
                {
                    var externalApp = Config.Current.System.ExternalAppCollection[i];
                    var header = MenuItemTools.IntegerToAccessKey(i + 1) + " " + MenuItemTools.EscapeMenuItemString(externalApp.DisplayName);
                    var parameter = parameterFactory.CreateParameter(externalApp);
                    items.Add(new MenuItem() { Header = header, ToolTip = externalApp.Command, Command = command, CommandParameter = parameter });
                }
            }
            else
            {
                items.Add(new MenuItem() { Header = TextResources.GetString("Word.ItemNone"), IsEnabled = false });
            }

            items.Add(new Separator());
            items.Add(new MenuItem() { Header = TextResources.GetString("BookshelfItem.Menu.ExternalAppOption"), Command = dialogCommand });
        }
    }


    public class ExternalAppCommandParameterFactory : ICommandParameterFactory<ExternalApp>
    {
        public object CreateParameter(ExternalApp folder)
        {
            return folder;
        }
    }
}
