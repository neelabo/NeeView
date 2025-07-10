using NeeView.Properties;

namespace NeeView
{
    public class ViewScrollRightCommand : CommandElement
    {
        public ViewScrollRightCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollRight(e.Parameter.Cast<ViewScrollCommandParameter>());
        }
    }
}
