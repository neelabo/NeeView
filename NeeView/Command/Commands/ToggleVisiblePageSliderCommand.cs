using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePageSliderCommand : CommandElement
    {
        public ToggleVisiblePageSliderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SliderConfig.IsEnabled)) { Source = Config.Current.Slider };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Slider.IsEnabled ? TextResources.GetString("ToggleVisiblePageSliderCommand.Off") : TextResources.GetString("ToggleVisiblePageSliderCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.Slider.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.Slider.IsEnabled = !Config.Current.Slider.IsEnabled;
            }
        }
    }
}
