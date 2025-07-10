using NeeView.Properties;

namespace NeeView
{
    public class ToggleBookOrderCommand : CommandElement
    {
        public ToggleBookOrderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.ToggleFolderOrder();
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookshelfFolderList.Current.GetNextFolderOrder().ToAliasName();
        }
    }
}
