using NeeView.Properties;

namespace NeeView
{
    public class CancelFullScreenCommand : CommandElement
    {
        public CancelFullScreenCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.SetFullScreen(sender, false);
        }
    }
}
