using NeeView.Properties;
using System;
using System.Windows.Data;


namespace NeeView
{
    [Obsolete, Alternative("nv.Command.ToggleHideLeftPanel, ToggleHideRightPanel", 46, ErrorLevel = ScriptErrorLevel.Info)]
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
            var state = CommandElementTools.GetState(e, Config.Current.Panels.IsHidePanel);
            return GetStateExecuteMessage(state);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Panels.IsHidePanel = !Config.Current.Panels.IsHidePanel;
        }
    }
}
