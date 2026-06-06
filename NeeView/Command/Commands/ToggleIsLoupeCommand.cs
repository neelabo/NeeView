using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsLoupeCommand : CommandElement
    {
        public ToggleIsLoupeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(LoupeContext.IsEnabled)) { Mode = BindingMode.OneWay, Source = MainViewComponent.Current.LoupeContext };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewLoupeControl.GetLoupeMode());
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewLoupeControl.GetLoupeMode());
            MainViewComponent.Current.ViewLoupeControl.SetLoupeMode(state);
        }
    }
}
