using NeeView.Properties;

namespace NeeView
{
    public class DeleteFileCommand : CommandElement
    {
        public DeleteFileCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Delete");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanDeleteFile();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookOperation.Current.Control.DeleteFileAsync();
        }
    }
}
