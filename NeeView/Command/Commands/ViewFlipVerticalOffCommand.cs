using NeeView.Properties;

namespace NeeView
{
    public class ViewFlipVerticalOffCommand : CommandElement
    {
        public ViewFlipVerticalOffCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.FlipVertical(false);
        }
    }
}
