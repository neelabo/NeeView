using NeeView.Properties;

namespace NeeView
{
    public class CopyBookCommand : CommandElement
    {
        public CopyBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanCopyBookToClipboard();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.BookControl.CopyBookToClipboard();
        }
    }
}
