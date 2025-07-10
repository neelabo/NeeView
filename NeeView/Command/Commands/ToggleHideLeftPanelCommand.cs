using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideLeftPanelCommand : CommandElement
    {
        public ToggleHideLeftPanelCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsHideLeftPanel)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Panels.IsHideLeftPanel ? TextResources.GetString("ToggleHideLeftPanelCommand.Off") : TextResources.GetString("ToggleHideLeftPanelCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Panels.IsHideLeftPanel = !Config.Current.Panels.IsHideLeftPanel;
        }
    }


}
