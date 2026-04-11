using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleAutoScrollCommand : CommandElement
    {
        public ToggleAutoScrollCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("MiddleClick");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainViewComponent.Current.ViewAutoScrollControl.IsAutoScrollMode)) { Source = MainViewComponent.Current.ViewAutoScrollControl, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewAutoScrollControl.GetAutoScrollMode() ? TextResources.GetString("ToggleAutoScrollCommand.Off") : TextResources.GetString("ToggleAutoScrollCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                var isEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
                MainViewComponent.Current.ViewAutoScrollControl.SetAutoScrollMode(isEnabled);
            }
            else
            {
                MainViewComponent.Current.ViewAutoScrollControl.ToggleAutoScrollMode();
            }
        }
    }
}
