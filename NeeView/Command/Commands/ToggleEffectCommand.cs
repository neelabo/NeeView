using NeeView.Effects;
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
            return Config.Current.ImageEffect.IsEnabled ? TextResources.GetString("ToggleEffectCommand.Off") : TextResources.GetString("ToggleEffectCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageEffect.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.ImageEffect.IsEnabled = !Config.Current.ImageEffect.IsEnabled;
            }
        }
    }
}
