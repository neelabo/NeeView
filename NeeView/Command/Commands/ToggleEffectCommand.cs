using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleEffectCommand : CommandElement
    {
        public ToggleEffectCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.ShortCutKey = new ShortcutKey("Ctrl+E");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageEffectConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageEffect };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageEffect.IsEnabled);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.ImageEffect.IsEnabled);
            Config.Current.ImageEffect.IsEnabled = state;
        }
    }
}
