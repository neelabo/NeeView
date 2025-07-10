using NeeView.Properties;

namespace NeeView
{
    public class OpenExplorerCommand : CommandElement
    {
        public OpenExplorerCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanOpenFilePlace();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.OpenFilePlace();
        }
    }
}
