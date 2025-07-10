using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByTimeStampACommand : CommandElement
    {
        public SetBookOrderByTimeStampACommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.TimeStamp);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStamp);
        }
    }
}
