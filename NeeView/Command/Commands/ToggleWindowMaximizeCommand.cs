using NeeView.Properties;

namespace NeeView
{
    public class ToggleWindowMaximizeCommand : CommandElement
    {
        public ToggleWindowMaximizeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.ToggleWindowMaximize(sender);
        }
    }
}
