using NeeView.Properties;

namespace NeeView
{
    public class ExportBookAsCommand : CommandElement
    {
        public ExportBookAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ShortCutKey = new ShortcutKey("Ctrl+Shift+B");

            //this.ParameterSource = new CommandParameterSource(new ExportBookAsCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanExport();
        }

        public override async void Execute(object? sender, CommandContext e)
        {
            //BookOperation.Current.Control.ExportBookDialog(e.Parameter.Cast<ExportBookAsCommandParameter>());

            var exporter = new ExportBook(BookOperation.Current);
            await exporter.RunAsync(true, System.Threading.CancellationToken.None);
        }
    }
}
