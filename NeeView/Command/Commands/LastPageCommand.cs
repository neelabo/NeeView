namespace NeeView
{
    public class LastPageCommand : CommandElement
    {
        public LastPageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("Ctrl+Left");
            this.MouseGesture = new MouseSequence("UL");
            this.IsShowMessage = true;
            this.PairPartner = "FirstPage";

            // FirstPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveToLast(this);
        }
    }
}
