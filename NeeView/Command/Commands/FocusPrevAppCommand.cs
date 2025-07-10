using NeeView.Properties;

namespace NeeView
{
    public class FocusPrevAppCommand : CommandElement
    {
        public FocusPrevAppCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.ShortCutKey = new ShortcutKey("Ctrl+Shift+Tab");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            WindowActivator.NextActivate(-1);
        }
    }

}
