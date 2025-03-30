using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public static class BookCommandTools
    {
        public static void UpdateSelectArchiverMenu(ItemCollection items)
        {
            items.Clear();
            foreach (var archiver in SelectableArchiverList.Current.GetLatestArchivers())
            {
                items.Add(new MenuItem() { Header = archiver.ToDisplayString(), Command = ReloadWithCommand.Command, CommandParameter = archiver });
            }
        }

        public static MenuItem CreateSelectArchiverMenuItem(string header)
        {
            var item = new MenuItem();
            item.Header = header;
            item.SetBinding(MenuItem.ItemsSourceProperty, new Binding(nameof(SelectableArchiverList.Archivers)) { Source = SelectableArchiverList.Current });
            item.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(SelectableArchiverList.IsEnableArchives)) { Source = SelectableArchiverList.Current });
            item.ItemContainerStyle = App.Current.MainWindow.Resources["SelectArchiverMenuItemContainerStyle"] as Style;
            item.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    SelectableArchiverList.Current.Update();
                }
            };
            return item;
        }
    }
}
