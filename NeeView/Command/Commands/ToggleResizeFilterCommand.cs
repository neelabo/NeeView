using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleResizeFilterCommand : CommandElement
    {
        public ToggleResizeFilterCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.ShortCutKey = new ShortcutKey("Ctrl+R");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageResizeFilterConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageResizeFilter };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageResizeFilter.IsEnabled);
            return GetStateExecuteMessage(state);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageResizeFilter.IsEnabled);
            Config.Current.ImageResizeFilter.IsEnabled = state;
        }
    }
}
