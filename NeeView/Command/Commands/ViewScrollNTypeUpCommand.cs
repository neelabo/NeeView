using NeeView.Properties;

namespace NeeView
{
    public class ViewScrollNTypeUpCommand : CommandElement
    {
        public ViewScrollNTypeUpCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollNTypeUp(e.Parameter.Cast<ViewScrollNTypeCommandParameter>());
        }
    }

}
