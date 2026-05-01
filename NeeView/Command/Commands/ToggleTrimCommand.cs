using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleTrimCommand : CommandElement
    {
        public ToggleTrimCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageTrimConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageTrim };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageTrim.IsEnabled);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageTrim.IsEnabled);
            Config.Current.ImageTrim.IsEnabled = state;
        }
    }
}
