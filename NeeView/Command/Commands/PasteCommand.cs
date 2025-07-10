using NeeView.Properties;

namespace NeeView
{
    public class PasteCommand : CommandElement
    {
        public PasteCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+V");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return ContentDropManager.Current.CanLoadFromClipboard();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ContentDropManager.Current.LoadFromClipboard(sender ?? this);
        }
    }
}
