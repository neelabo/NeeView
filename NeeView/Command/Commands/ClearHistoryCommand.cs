namespace NeeView
{
    public class ClearHistoryCommand : CommandElement
    {
        public ClearHistoryCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookHistoryCollection.Current.Clear();
        }
    }
}
