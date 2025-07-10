using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideMenuCommand : CommandElement
    {
        public ToggleHideMenuCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MenuBarConfig.IsHideMenu)) { Source = Config.Current.MenuBar };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.MenuBar.IsHideMenu ? TextResources.GetString("ToggleHideMenuCommand.Off") : TextResources.GetString("ToggleHideMenuCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHideMenu();
        }
    }
}
