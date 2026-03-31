using NeeView.Properties;

namespace NeeView
{
    public class ExportBookAsCommand : CommandElement
    {
        public ExportBookAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanExportBook();
        }

        public override async void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.BookControl.ExportBook();
        }
    }
}
