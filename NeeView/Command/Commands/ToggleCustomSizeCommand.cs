using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleCustomSizeCommand : CommandElement
    {
        public ToggleCustomSizeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageCustomSizeConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageCustomSize };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageCustomSize.IsEnabled);
            return GetStateExecuteMessage(state);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageCustomSize.IsEnabled);
            Config.Current.ImageCustomSize.IsEnabled = state;
        }
    }
}
