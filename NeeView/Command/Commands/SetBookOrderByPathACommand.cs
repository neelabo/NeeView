using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByPathACommand : CommandElement
    {
        public SetBookOrderByPathACommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Path);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Path);
        }
    }
}
