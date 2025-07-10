using NeeView.Properties;

namespace NeeView
{
    public class NextPageCommand : CommandElement
    {
        public NextPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("Left,LeftClick");
            this.TouchGesture = new TouchGesture("TouchL1,TouchL2");
            this.MouseGesture = new MouseSequence("L");
            this.IsShowMessage = false;
            this.PairPartner = "PrevPage";

            // PrevPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveNext(this);
        }
    }
}
