using NeeView.Properties;

namespace NeeView
{
    public class RenameBookCommand : CommandElement
    {
        public RenameBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanRenameBook();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.BookControl.RenameBook();
        }
    }
}
