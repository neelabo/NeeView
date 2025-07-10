using NeeView.Properties;

namespace NeeView
{
    public class CloseApplicationCommand : CommandElement
    {
        public CloseApplicationCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindow.Current.Close();
        }
    }
}
