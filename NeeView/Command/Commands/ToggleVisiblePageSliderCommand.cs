using NeeView.Properties;
using NeeView.Windows;
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
            var state = CommandElementTools.GetState(e, Config.Current.Slider.IsEnabled, MainWindow.Current.IsPageSliderVisible);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.Slider.IsEnabled, MainWindow.Current.IsPageSliderVisible);
            if (e.ByMenu)
            {
                Config.Current.Slider.IsEnabled = state;
            }
            else
            {
                MainWindowModel.Current.SetPageSliderVisible(state.ToStateRequest());
            }
        }
    }
}
