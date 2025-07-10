using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideRightPanelCommand : CommandElement
    {
        public ToggleHideRightPanelCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsHideRightPanel)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Panels.IsHideRightPanel ? TextResources.GetString("ToggleHideRightPanelCommand.Off") : TextResources.GetString("ToggleHideRightPanelCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Panels.IsHideRightPanel = !Config.Current.Panels.IsHideRightPanel;
        }
    }


}
