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
            var state = CommandElementTools.GetState(e, Config.Current.MenuBar.IsHideMenu);
            return GetStateExecuteMessage(state);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHideMenu();
        }
    }
}
