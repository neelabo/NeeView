using NeeView.Properties;

namespace NeeView
{
    public class ToggleWindowMinimizeCommand : CommandElement
    {
        public ToggleWindowMinimizeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.ToggleWindowMinimize(sender);
        }
    }
}
