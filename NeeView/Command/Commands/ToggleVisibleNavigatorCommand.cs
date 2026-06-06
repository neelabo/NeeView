using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleNavigatorCommand : CommandElement
    {
        public ToggleVisibleNavigatorCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("N");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleNavigator)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SidePanelFrame.Current.GetVisibleNavigator(e.ByMenu));
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                SidePanelFrame.Current.SetVisibleNavigator(toggleMode == ToggleMode.On, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleNavigator(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
