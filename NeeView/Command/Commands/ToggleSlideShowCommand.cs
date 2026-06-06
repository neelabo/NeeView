using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleSlideShowCommand : CommandElement
    {
        public ToggleSlideShowCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("F5");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(SlideShow.IsPlaying)) { Source = SlideShow.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SlideShow.Current.IsPlaying);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SlideShow.Current.IsPlaying);
            SlideShow.Current.SetPlaying(state);
        }
    }
}
