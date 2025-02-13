using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleResizeFilterCommand : CommandElement
    {
        public ToggleResizeFilterCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Effect");
            this.ShortCutKey = new ShortcutKey("Ctrl+R");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageResizeFilterConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageResizeFilter };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.ImageResizeFilter.IsEnabled ? Properties.TextResources.GetString("ToggleResizeFilterCommand.Off") : Properties.TextResources.GetString("ToggleResizeFilterCommand.On");
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
                Config.Current.ImageResizeFilter.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.ImageResizeFilter.IsEnabled = !Config.Current.ImageResizeFilter.IsEnabled;
            }
        }
    }
}
