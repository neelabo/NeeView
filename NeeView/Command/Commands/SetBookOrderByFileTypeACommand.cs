using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileTypeACommand : CommandElement
    {
        public SetBookOrderByFileTypeACommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileType);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileType);
        }
    }
}
