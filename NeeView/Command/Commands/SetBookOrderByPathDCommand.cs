using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByPathDCommand : CommandElement
    {
        public SetBookOrderByPathDCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.PathDescending);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.PathDescending);
        }
    }
}
