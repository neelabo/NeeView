using NeeView.Properties;

namespace NeeView
{
    public class ExportImageCommand : CommandElement
    {
        public ExportImageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Shift+Ctrl+S");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ExportImageCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanExport();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.Export(e.Parameter.Cast<ExportImageCommandParameter>());
        }
    }
}
