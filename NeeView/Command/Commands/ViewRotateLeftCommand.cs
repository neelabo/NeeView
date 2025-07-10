using NeeView.Properties;

namespace NeeView
{
    public class ViewRotateLeftCommand : CommandElement
    {
        public ViewRotateLeftCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ViewRotateLeft(e.Parameter.Cast<ViewRotateCommandParameter>());
        }
    }
}
