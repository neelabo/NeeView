using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleSideBarCommand : CommandElement
    {
        public ToggleVisibleSideBarCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsSideBarEnabled)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Panels.IsSideBarEnabled ? TextResources.GetString("ToggleVisibleSideBarCommand.Off") : TextResources.GetString("ToggleVisibleSideBarCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Panels.IsSideBarEnabled = !Config.Current.Panels.IsSideBarEnabled;
        }
    }
}
