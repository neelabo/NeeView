using System;
using System.Globalization;
using System.Windows.Input;

namespace NeeView
{
    public static class CommandTools
    {
        /// <summary>
        /// コマンド有効判定
        /// </summary>
        /// <param name="commandName">コマンドID</param>
        /// <returns>コマンド有効なら true</returns>
        public static bool IsCommandEnabled(string? commandName)
        {
            if (commandName is null) return false;

            return CommandTable.Current.ContainsKey(commandName);
        }


        /// <summary>
        /// コマンドの ToolTip 文字列を作成
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        public static string CreateToolTipText(string name, Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            var text = ResourceService.GetString(name);
            var gesture = new KeyGestureSource(key, modifiers)?.GetDisplayString();
            return string.IsNullOrEmpty(gesture) ? text : $"{text} ({gesture})";
        }

        /// <summary>
        /// コマンド表示名取得
        /// </summary>
        /// <param name="commandName">コマンドID</param>
        /// <param name="getCommandTextFunc">テキスト取得関数</param>
        /// <returns>コマンド表示名</returns>
        public static string GetCommandText(string? commandName, Func<CommandElement, string> getCommandTextFunc)
        {
            if (commandName is null)
            {
                return "(none)";
            }

            if (CommandTable.Current.TryGetValue(commandName, out CommandElement? command))
            {
                return getCommandTextFunc(command);
            }

            if (ScriptCommand.IsScriptCommandName(commandName))
            {
                return commandName.Replace(CommandNameSource.Separator, ' ').Replace('_', ' ');
            }

            var cloneName = CommandNameSource.Parse(commandName);
            if (cloneName.IsClone)
            {
                if (CommandTable.Current.TryGetValue(cloneName.Name, out CommandElement? commandSource))
                {
                    return getCommandTextFunc(commandSource) + " " + cloneName.Number.ToString(CultureInfo.InvariantCulture);
                }
            }

            return "(none)";
        }
    }




}
