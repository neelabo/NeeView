using NeeView.Properties;

namespace NeeView
{
    public class StretchWindowCommand : CommandElement
    {
        public StretchWindowCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.StretchWindow();
        }
    }
}
