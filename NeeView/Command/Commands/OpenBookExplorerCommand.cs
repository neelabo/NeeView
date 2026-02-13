using NeeView.Properties;

namespace NeeView
{
    public class OpenBookExplorerCommand : CommandElement
    {
        public OpenBookExplorerCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanOpenBookPlace();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.BookControl.OpenBookPlace();
        }
    }
}
