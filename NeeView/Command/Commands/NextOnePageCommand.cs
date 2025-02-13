namespace NeeView
{
    public class NextOnePageCommand : CommandElement
    {
        public NextOnePageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.MouseGesture = new MouseSequence("RL");
            this.IsShowMessage = false;
            this.PairPartner = "PrevOnePage";

            // PrevOnePage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveNextOne(this);
        }
    }
}
