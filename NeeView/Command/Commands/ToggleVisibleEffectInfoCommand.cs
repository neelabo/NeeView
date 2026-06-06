using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleEffectInfoCommand : CommandElement
    {
        public ToggleVisibleEffectInfoCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("E");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleEffectInfo)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SidePanelFrame.Current.GetVisibleEffectInfo(e.ByMenu));
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                SidePanelFrame.Current.SetVisibleEffectInfo(toggleMode == ToggleMode.On, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleEffectInfo(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
