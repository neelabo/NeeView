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
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleNavigator)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleNavigator ? TextResources.GetString("ToggleVisibleNavigatorCommand.Off") : TextResources.GetString("ToggleVisibleNavigatorCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleNavigator(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleNavigator(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
