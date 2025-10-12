using Jint.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public static partial class MenuExtensions
    {
        // メニューコントロールのジェスチャーテキスト更新
        public static void UpdateInputGestureText(this ItemsControl control)
        {
            if (control == null) return;

            foreach (var item in control.Items.OfType<MenuItem>())
            {
                if (item.Command is RoutedCommand command)
                {
                    var fixedCommand = RoutedCommandTable.Current.GetFixedRoutedCommand(command.Name, true) ?? command;

                    string text = "";
                    foreach (InputGesture gesture in fixedCommand.InputGestures)
                    {
                        // キーショートカットのみ対応
                        if (gesture is KeyGesture or KeyExGesture)
                        {
                            text += ((text.Length > 0) ? ", " : "") + InputGestureDisplayString.GetDisplayString(gesture);
                        }
                    }
                    item.InputGestureText = text;
                }

                UpdateInputGestureText(item);
            }
        }
    }
}
