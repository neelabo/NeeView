using NeeView.Properties;

namespace NeeView
{
    public class CutBookCommand : CommandElement
    {
        public CutBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanCutBookToClipboard();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.BookControl.CutBookToClipboard();
        }
    }
}
