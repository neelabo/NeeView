using NeeView.Properties;

namespace NeeView
{
    public class ToggleStretchModeReverseCommand : CommandElement
    {
        public ToggleStretchModeReverseCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ImageScale");
            this.ShortCutKey = new ShortcutKey("LeftButton+WheelUp");
            this.IsShowMessage = true;

            // "ToggleStretchMode"
            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewPropertyControl.GetToggleStretchModeReverse(e.Parameter.Cast<ToggleStretchModeCommandParameter>()).ToAliasName();
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.View.StretchMode = MainViewComponent.Current.ViewPropertyControl.GetToggleStretchModeReverse(e.Parameter.Cast<ToggleStretchModeCommandParameter>());
        }
    }
}
