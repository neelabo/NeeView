using NeeView.Properties;

namespace NeeView
{
    public class PrevBookCommand : CommandElement
    {
        public PrevBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = new ShortcutKey("Up");
            this.MouseGesture = new MouseSequence("LU");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return Config.Current.Book.IsPrioritizeBookMove || !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookshelfFolderList.Current.PrevFolder(Config.Current.Book.IsPrioritizeBookMove);
        }
    }
}
