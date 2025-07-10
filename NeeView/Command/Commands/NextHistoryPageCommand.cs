using NeeView.Properties;

namespace NeeView
{
    public class NextHistoryPageCommand : CommandElement
    {
        public NextHistoryPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.ShortCutKey = new ShortcutKey("Shift+Back");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return PageHistory.Current.CanMoveToNext();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            PageHistory.Current.MoveToNext();
        }
    }
}
