using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsLoupeCommand : CommandElement
    {
        public ToggleIsLoupeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(LoupeContext.IsEnabled)) { Mode = BindingMode.OneWay, Source = MainViewComponent.Current.LoupeContext };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewLoupeControl.GetLoupeMode() ? TextResources.GetString("ToggleIsLoupeCommand.Off") : TextResources.GetString("ToggleIsLoupeCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewLoupeControl.SetLoupeMode(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                MainViewComponent.Current.ViewLoupeControl.ToggleLoupeMode();
            }
        }
    }
}
