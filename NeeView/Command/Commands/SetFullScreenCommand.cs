using NeeView.Properties;

namespace NeeView
{
    public class SetFullScreenCommand : CommandElement
    {
        public SetFullScreenCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.SetFullScreen(sender, true);
        }
    }
}
