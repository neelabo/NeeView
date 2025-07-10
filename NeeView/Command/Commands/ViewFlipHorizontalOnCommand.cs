using NeeView.Properties;

namespace NeeView
{
    public class ViewFlipHorizontalOnCommand : CommandElement
    {
        public ViewFlipHorizontalOnCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.FlipHorizontal(true);
        }
    }
}
