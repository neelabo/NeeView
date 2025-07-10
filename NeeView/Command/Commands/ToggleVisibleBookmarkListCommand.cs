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
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleBookmarkList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleBookmarkList ? TextResources.GetString("ToggleVisibleBookmarkListCommand.Off") : TextResources.GetString("ToggleVisibleBookmarkListCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleBookmarkList(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleBookmarkList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
