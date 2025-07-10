using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHidePageSliderCommand : CommandElement
    {
        public ToggleHidePageSliderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SliderConfig.IsHidePageSlider)) { Source = Config.Current.Slider };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Slider.IsHidePageSlider ? TextResources.GetString("ToggleHidePageSliderCommand.Off") : TextResources.GetString("ToggleHidePageSliderCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return Config.Current.Slider.IsEnabled;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHidePageSlider();
        }
    }
}
