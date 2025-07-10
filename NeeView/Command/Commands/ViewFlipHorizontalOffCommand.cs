using NeeView.Properties;

namespace NeeView
{
    public class ViewFlipHorizontalOffCommand : CommandElement
    {
        public ViewFlipHorizontalOffCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.FlipHorizontal(false);
        }
    }
}
