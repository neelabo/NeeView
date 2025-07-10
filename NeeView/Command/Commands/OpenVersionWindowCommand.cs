using NeeView.Properties;

namespace NeeView
{
    public class OpenVersionWindowCommand : CommandElement
    {
        public OpenVersionWindowCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }
        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.OpenVersionWindow();
        }
    }
}
