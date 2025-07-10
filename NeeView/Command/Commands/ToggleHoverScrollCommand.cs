using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHoverScrollCommand : CommandElement
    {
        public ToggleHoverScrollCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MouseConfig.IsHoverScroll)) { Source = Config.Current.Mouse, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Mouse.IsHoverScroll ? TextResources.GetString("ToggleHoverScrollCommand.Off") : TextResources.GetString("ToggleHoverScrollCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.Mouse.IsHoverScroll = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.Mouse.IsHoverScroll = !Config.Current.Mouse.IsHoverScroll;
            }
        }
    }
}
