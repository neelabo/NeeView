using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public abstract class MoreMenuDescription
    {
        public abstract ContextMenu Create();

        public virtual ContextMenu Update(ContextMenu menu)
        {
            return menu;
        }


        protected MenuItem CreateCheckMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
        }

        protected MenuItem CreateCommandMenuItem(string header, ICommand command, Binding? binding = null)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }
            return item;
        }

        protected MenuItem CreateCommandMenuItem(string header, string command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            var binding = CommandTable.Current.GetElement(command).CreateIsCheckedBinding();
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }
    }
}
