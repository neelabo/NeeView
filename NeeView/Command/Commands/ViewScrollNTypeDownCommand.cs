using NeeView.Properties;

namespace NeeView
{
    public class ViewScrollNTypeDownCommand : CommandElement
    {
        public ViewScrollNTypeDownCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            // ViewScrollNTypeUpCommand
            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollNTypeDown(e.Parameter.Cast<ViewScrollNTypeCommandParameter>());
        }
    }

}
