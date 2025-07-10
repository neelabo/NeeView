using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByEntryTimeDCommand : CommandElement
    {
        public SetBookOrderByEntryTimeDCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.EntryTimeDescending);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTimeDescending);
        }
    }
}
