using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileNameACommand : CommandElement
    {
        public SetBookOrderByFileNameACommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileName);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileName);
        }
    }
}
