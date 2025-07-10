using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderBySizeACommand : CommandElement
    {
        public SetBookOrderBySizeACommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Size);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Size);
        }
    }
}
