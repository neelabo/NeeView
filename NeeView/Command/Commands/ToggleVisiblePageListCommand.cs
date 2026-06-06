using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePageListCommand : CommandElement
    {
        public ToggleVisiblePageListCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("P");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisiblePageList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SidePanelFrame.Current.GetVisiblePageList(e.ByMenu));
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                SidePanelFrame.Current.SetVisiblePageList(toggleMode == ToggleMode.On, true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisiblePageList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
