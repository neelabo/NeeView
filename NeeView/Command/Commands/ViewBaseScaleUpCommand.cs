using NeeView.Properties;

namespace NeeView
{
    public class ViewBaseScaleUpCommand : CommandElement
    {
        public ViewBaseScaleUpCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScaleUp(ScaleType.BaseScale, e.Parameter.Cast<ViewScaleCommandParameter>());
        }
    }

}
