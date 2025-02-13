using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowScaleUpCommand : CommandElement
    {
        public ToggleStretchAllowScaleUpCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ImageScale");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ViewConfig.AllowStretchScaleUp)) { Source = Config.Current.View };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return this.Text + (Config.Current.View.AllowStretchScaleUp ? " OFF" : " ");
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
                Config.Current.View.AllowStretchScaleUp = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.View.AllowStretchScaleUp = !Config.Current.View.AllowStretchScaleUp;
            }
        }
    }
}
