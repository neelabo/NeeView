using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformCommand : CommandElement
    {
        public SetStretchModeUniformCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ImageScale");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.Uniform);
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return this.Text + (MainViewComponent.Current.ViewPropertyControl.TestStretchMode(PageStretchMode.Uniform, (e.Parameter.Cast<StretchModeCommandParameter>()).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewPropertyControl.SetStretchMode(PageStretchMode.Uniform, (e.Parameter.Cast<StretchModeCommandParameter>()).IsToggle, true);
        }
    }
}
