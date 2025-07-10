using NeeView.Properties;

namespace NeeView
{
    public class PrevHistoryPageCommand : CommandElement
    {
        public PrevHistoryPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("Back");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return PageHistory.Current.CanMoveToPrevious();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            PageHistory.Current.MoveToPrevious();
        }
    }
}
