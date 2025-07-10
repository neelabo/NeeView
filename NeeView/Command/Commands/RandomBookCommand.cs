using NeeView.Properties;

namespace NeeView
{
    public class RandomBookCommand : CommandElement
    {
        public RandomBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookMove");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookshelfFolderList.Current.RandomFolder();
        }
    }

}
