using NeeView.Properties;

namespace NeeView
{
    public class ViewScrollLeftCommand : CommandElement
    {
        public ViewScrollLeftCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollLeft(e.Parameter.Cast<ViewScrollCommandParameter>());
        }
    }
}
