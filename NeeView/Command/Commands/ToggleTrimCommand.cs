using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleTrimCommand : CommandElement
    {
        public ToggleTrimCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageTrimConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageTrim };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.ImageTrim.IsEnabled ? Properties.TextResources.GetString("ToggleTrimCommand.Off") : Properties.TextResources.GetString("ToggleTrimCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageTrim.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.ImageTrim.IsEnabled = !Config.Current.ImageTrim.IsEnabled;
            }
        }
    }
}
