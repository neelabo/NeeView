using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleNearestNeighborCommand : CommandElement
    {
        public ToggleNearestNeighborCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageDotKeepConfig.IsEnabled)) { Source = Config.Current.ImageDotKeep };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.ImageDotKeep.IsEnabled ? Properties.TextResources.GetString("ToggleNearestNeighborCommand.Off") : Properties.TextResources.GetString("ToggleNearestNeighborCommand.On");
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
                Config.Current.ImageDotKeep.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.ImageDotKeep.IsEnabled = !Config.Current.ImageDotKeep.IsEnabled;
            }
        }
    }
}
