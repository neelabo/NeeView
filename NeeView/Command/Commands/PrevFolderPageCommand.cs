using NeeView.Properties;

namespace NeeView
{
    public class PrevFolderPageCommand : CommandElement
    {
        public PrevFolderPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.IsShowMessage = true;
            this.PairPartner = "NextFolderPage";

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
            BookOperation.Current.Control.MovePrevFolder(this, this.IsShowMessage);
        }
    }
}
