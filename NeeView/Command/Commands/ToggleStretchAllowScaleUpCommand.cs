using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowScaleUpCommand : CommandElement
    {
        public ToggleStretchAllowScaleUpCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ImageScale");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ViewConfig.AllowStretchScaleUp)) { Source = Config.Current.View };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.View.AllowStretchScaleUp);
            return GetStateExecuteMessage(state);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.View.AllowStretchScaleUp);
            Config.Current.View.AllowStretchScaleUp = state;
        }
    }
}
