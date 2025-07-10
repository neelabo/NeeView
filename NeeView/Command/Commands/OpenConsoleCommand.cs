using NeeView.Properties;

namespace NeeView
{
    public class OpenConsoleCommand : CommandElement
    {
        public OpenConsoleCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ConsoleWindowManager.Current.OpenWindow();
        }
    }
}
