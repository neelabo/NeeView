using NeeView.Properties;

namespace NeeView
{
    public class FocusPageListSearchBoxCommand : CommandElement
    {
        public FocusPageListSearchBoxCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            SidePanelFrame.Current.FocusPageListSearchBox(e.Options.HasFlag(CommandOption.ByMenu));
        }
    }
}
