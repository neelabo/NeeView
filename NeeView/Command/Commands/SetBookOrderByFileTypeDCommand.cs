using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileTypeDCommand : CommandElement
    {
        public SetBookOrderByFileTypeDCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileTypeDescending);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileTypeDescending);
        }
    }
}
