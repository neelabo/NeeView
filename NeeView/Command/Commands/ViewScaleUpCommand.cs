using NeeView.Properties;

namespace NeeView
{
    public class ViewScaleUpCommand : CommandElement
    {
        public ViewScaleUpCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("RightButton+WheelUp");
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScaleUp(e.Parameter.Cast<ViewScaleCommandParameter>());
        }
    }
}
