using NeeView.Properties;

namespace NeeView
{
    public class PrevHistoryCommand : CommandElement
    {
        public PrevHistoryCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookMove");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return HistoryList.Current.CanPrevHistory();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            HistoryList.Current.PrevHistory();
        }
    }
}
