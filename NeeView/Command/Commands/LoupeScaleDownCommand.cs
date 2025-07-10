using NeeView.Properties;

namespace NeeView
{
    public class LoupeScaleDownCommand : CommandElement
    {
        public LoupeScaleDownCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewLoupeControl.GetLoupeMode();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewLoupeControl.LoupeZoomOut();
        }
    }
}
