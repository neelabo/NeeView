namespace NeeView
{
    public class JumpRandomPageCommand : CommandElement
    {
        public JumpRandomPageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveToRandom(this);
        }
    }
}
