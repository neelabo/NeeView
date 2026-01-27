using NeeView.Properties;

namespace NeeView
{
    public class CopyFileCommand : CommandElement
    {
        public CopyFileCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+C");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new CopyFileCommandParameter());

        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanCopyToClipboard(e.Parameter.Cast<CopyFileCommandParameter>());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.CopyToClipboard(e.Parameter.Cast<CopyFileCommandParameter>());
        }
    }


}
