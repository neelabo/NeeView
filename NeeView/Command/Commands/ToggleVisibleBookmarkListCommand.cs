using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleBookmarkListCommand : CommandElement
    {
        public ToggleVisibleBookmarkListCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("D");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleBookmarkList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SidePanelFrame.Current.GetVisibleBookmarkList(e.ByMenu));
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                SidePanelFrame.Current.SetVisibleBookmarkList(toggleMode == ToggleMode.On, true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleBookmarkList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
