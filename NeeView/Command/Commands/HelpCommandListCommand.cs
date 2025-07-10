using NeeView.Properties;

namespace NeeView
{
    public class HelpCommandListCommand : CommandElement
    {
        public HelpCommandListCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            CommandTable.Current.OpenCommandListHelp();
        }
    }
}
