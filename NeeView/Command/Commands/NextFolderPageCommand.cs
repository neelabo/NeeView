namespace NeeView
{
    public class NextFolderPageCommand : CommandElement
    {
        public NextFolderPageCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Move");
            this.IsShowMessage = true;
            this.PairPartner = "PrevFolderPage";

            // PrevFolderPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return "";
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveNextFolder(this, this.IsShowMessage);
        }
    }
}
