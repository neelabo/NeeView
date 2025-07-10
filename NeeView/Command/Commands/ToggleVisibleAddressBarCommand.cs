using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleAddressBarCommand : CommandElement
    {
        public ToggleVisibleAddressBarCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MenuBarConfig.IsAddressBarEnabled)) { Source = Config.Current.MenuBar };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.MenuBar.IsAddressBarEnabled ? TextResources.GetString("ToggleVisibleAddressBarCommand.Off") : TextResources.GetString("ToggleVisibleAddressBarCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleVisibleAddressBar();
        }
    }
}
