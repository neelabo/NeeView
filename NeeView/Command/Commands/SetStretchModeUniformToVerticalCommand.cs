using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformToVerticalCommand : CommandElement
    {
        public SetStretchModeUniformToVerticalCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ImageScale");
            this.IsShowMessage = true;

            // SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToVertical);
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return this.Text + (MainViewComponent.Current.ViewPropertyControl.TestStretchMode(PageStretchMode.UniformToVertical, (e.Parameter.Cast<StretchModeCommandParameter>()).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewPropertyControl.SetStretchMode(PageStretchMode.UniformToVertical, (e.Parameter.Cast<StretchModeCommandParameter>()).IsToggle, true);
        }
    }
}
