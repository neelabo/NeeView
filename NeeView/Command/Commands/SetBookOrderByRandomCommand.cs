using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByRandomCommand : CommandElement
    {
        public SetBookOrderByRandomCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Random);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Random);
        }
    }
}
