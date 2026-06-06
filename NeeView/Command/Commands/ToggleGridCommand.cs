using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleGridCommand : CommandElement
    {
        public ToggleGridCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageGridConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageGrid };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageGrid.IsEnabled);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageGrid.IsEnabled);
            Config.Current.ImageGrid.IsEnabled = state;
        }
    }
}
