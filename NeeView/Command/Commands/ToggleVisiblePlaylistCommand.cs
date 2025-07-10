using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePlaylistCommand : CommandElement
    {
        public ToggleVisiblePlaylistCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("M");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisiblePlaylist)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisiblePlaylist ? TextResources.GetString("ToggleVisiblePlaylistCommand.Off") : TextResources.GetString("ToggleVisiblePlaylistCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisiblePlaylist(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisiblePlaylist(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
