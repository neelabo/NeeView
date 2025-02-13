namespace NeeView
{
    public class PrevPageCommand : CommandElement
    {
        public PrevPageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("Right,RightClick");
            this.TouchGesture = new TouchGesture("TouchR1,TouchR2");
            this.MouseGesture = new MouseSequence("R");
            this.IsShowMessage = false;
            this.PairPartner = "NextPage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MovePrev(this);
        }
    }
}
