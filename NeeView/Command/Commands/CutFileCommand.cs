namespace NeeView
{
    public class CutFileCommand : CommandElement
    {
        public CutFileCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+X");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new CopyFileCommandParameter());

        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanCutToClipboard(e.Parameter.Cast<CopyFileCommandParameter>());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.CutToClipboard(e.Parameter.Cast<CopyFileCommandParameter>());
        }
    }

}
