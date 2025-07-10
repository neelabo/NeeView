using NeeView.Properties;

namespace NeeView
{
    public class ViewFlipVerticalOnCommand : CommandElement
    {
        public ViewFlipVerticalOnCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.FlipVertical(true);
        }
    }
}
