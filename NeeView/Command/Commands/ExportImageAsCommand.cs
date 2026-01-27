using NeeView.Properties;

namespace NeeView
{
    public class ExportImageAsCommand : CommandElement
    {
        public ExportImageAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+S");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportImageAsCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanExport();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.ExportDialog(e.Parameter.Cast<ExportImageAsCommandParameter>());
        }
    }
}
