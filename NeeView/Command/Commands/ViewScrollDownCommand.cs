using NeeView.Properties;

namespace NeeView
{
    public class ViewScrollDownCommand : CommandElement
    {
        public ViewScrollDownCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollDown(e.Parameter.Cast<ViewScrollCommandParameter>());
        }
    }


}
