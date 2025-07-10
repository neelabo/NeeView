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
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisiblePageList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisiblePageList ? TextResources.GetString("ToggleVisiblePageListCommand.Off") : TextResources.GetString("ToggleVisiblePageListCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisiblePageList(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisiblePageList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
