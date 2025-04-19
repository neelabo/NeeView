using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public static class RecentBookTools
    {
        public static void UpdateRecentBookMenu(ItemCollection items)
        {
            items.Clear();
            foreach (var book in RecentBookList.Current.GetBooks())
            {
                items.Add(new MenuItem() { Header = book.Name, Command = LoadCommand.Command, CommandParameter = book.Path });
            }
        }

        public static MenuItem CreateRecentBookMenuItem(string header)
        {
            var item = new MenuItem();
            item.Header = header;
            item.SetBinding(MenuItem.ItemsSourceProperty, new Binding(nameof(RecentBookList.Books)) { Source = RecentBookList.Current });
            item.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(RecentBookList.IsEnabled)) { Source = RecentBookList.Current });
            item.ItemContainerStyle = App.Current.MainWindow.Resources["HistoryMenuItemContainerStyle"] as Style;
            item.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    RecentBookList.Current.Update();
                }
            };
            return item;
        }
    }

}
