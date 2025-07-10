using NeeView.Properties;

namespace NeeView
{
    public class HelpMainMenuCommand : CommandElement
    {
        public HelpMainMenuCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainMenuManual.OpenMainMenuManual();
        }
    }
}
