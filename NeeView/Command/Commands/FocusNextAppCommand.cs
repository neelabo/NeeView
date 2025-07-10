using NeeView.Properties;

namespace NeeView
{
    public class FocusNextAppCommand : CommandElement
    {
        public FocusNextAppCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.ShortCutKey = new ShortcutKey("Ctrl+Tab");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            WindowActivator.NextActivate(+1);
        }
    }

}
