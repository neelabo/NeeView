using NeeView.Properties;

namespace NeeView
{
    public class ViewPresetScrollCommand : CommandElement
    {
        public ViewPresetScrollCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewPresetScrollCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollToPreset(Parameter.Cast<ViewPresetScrollCommandParameter>());
        }
    }

}
