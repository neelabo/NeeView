using NeeView.Properties;

namespace NeeView
{
    public class NextScrollPageCommand : CommandElement
    {
        public NextScrollPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("WheelDown");
            this.IsShowMessage = false;
            this.PairPartner = "PrevScrollPage";

            // PrevScrollPage
            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.ScrollToNextFrame(sender, e.Parameter.Cast<ScrollPageCommandParameter>());
        }
    }


}
