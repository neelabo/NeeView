namespace NeeView
{
    public class NextBookCommand : CommandElement
    {
        public NextBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = new ShortcutKey("Down");
            this.MouseGesture = new MouseSequence("LD");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return Config.Current.Book.IsPrioritizeBookMove || !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookshelfFolderList.Current.NextFolder(Config.Current.Book.IsPrioritizeBookMove);
        }
    }
}
