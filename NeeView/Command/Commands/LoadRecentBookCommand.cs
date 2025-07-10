using NeeView.Properties;
using System.Windows.Controls;

namespace NeeView
{
    public class LoadRecentBookCommand : CommandElement
    {
        public LoadRecentBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return true;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.MainView.CommandMenu.OpenRecentBookMenu();
        }

        public override MenuItem? CreateMenuItem(bool isDefault)
        {
            return RecentBookTools.CreateRecentBookMenuItem(this.Menu);
        }
    }
}
