using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleCustomSizeCommand : CommandElement
    {
        public ToggleCustomSizeCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ImageScale");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageCustomSizeConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageCustomSize };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.ImageCustomSize.IsEnabled ? Properties.TextResources.GetString("ToggleCustomSizeCommand.Off") : Properties.TextResources.GetString("ToggleCustomSizeCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageCustomSize.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.ImageCustomSize.IsEnabled = !Config.Current.ImageCustomSize.IsEnabled;
            }
        }
    }
}
