using NeeView.Properties;
using System;
using System.Windows.Data;


namespace NeeView
{
    [Obsolete]
    public class ToggleHidePanelCommand : CommandElement
    {
        public ToggleHidePanelCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsHidePanel)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Panels.IsHidePanel ? TextResources.GetString("ToggleHidePanelCommand.Off") : TextResources.GetString("ToggleHidePanelCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Panels.IsHidePanel = !Config.Current.Panels.IsHidePanel;
        }
    }
}
