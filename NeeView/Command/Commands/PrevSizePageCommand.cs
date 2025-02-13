namespace NeeView
{
    public class PrevSizePageCommand : CommandElement
    {
        public PrevSizePageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.IsShowMessage = false;
            this.PairPartner = "NextSizePage";

            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MovePrevSize(this, e.Parameter.Cast<MoveSizePageCommandParameter>().Size);
        }
    }
}
