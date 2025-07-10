using NeeView.Properties;

namespace NeeView
{
    public class FocusBookmarkListCommand : CommandElement
    {
        public FocusBookmarkListCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.IsShowMessage = false;
        }
        public override void Execute(object? sender, CommandContext e)
        {
            SidePanelFrame.Current.FocusBookshelfBookmarkList(e.Options.HasFlag(CommandOption.ByMenu));
        }
    }
}
