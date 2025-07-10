using NeeView.Properties;

namespace NeeView
{
    public class TouchEmulateCommand : CommandElement
    {
        public TouchEmulateCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.TouchInputEmulate(sender);
        }
    }
}
