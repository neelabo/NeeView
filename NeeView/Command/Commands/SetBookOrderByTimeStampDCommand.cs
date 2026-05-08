using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByTimeStampDCommand : CommandElement
    {
        public SetBookOrderByTimeStampDCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.TimeStampDescending);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStampDescending);
        }
    }
}
