using NeeView.Properties;

namespace NeeView
{
    public class LoupeOffCommand : CommandElement
    {
        public LoupeOffCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewLoupeControl.SetLoupeMode(false);
        }
    }
}
