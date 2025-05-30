namespace NeeView
{
    public class RegisterBookmarkCommand : CommandElement
    {
        public RegisterBookmarkCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Bookmark");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanBookmark();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindow.Current.AddressBar.BookmarkPopup.IsOpen = true;
        }
    }
}
