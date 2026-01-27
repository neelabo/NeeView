using NeeView.Properties;

namespace NeeView
{
    public class ViewRotateRightCommand : CommandElement
    {
        public ViewRotateRightCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            // ViewRotateLeft
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ViewRotateRight(e.Parameter.Cast<ViewRotateCommandParameter>());
        }
    }
}
