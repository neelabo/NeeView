using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleNearestNeighborCommand : CommandElement
    {
        public ToggleNearestNeighborCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageDotKeepConfig.IsEnabled)) { Source = Config.Current.ImageDotKeep };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageDotKeep.IsEnabled);
            return GetStateExecuteMessage(state);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageDotKeep.IsEnabled);
            Config.Current.ImageDotKeep.IsEnabled = state;
        }
    }
}
