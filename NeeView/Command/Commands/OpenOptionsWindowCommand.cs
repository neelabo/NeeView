using NeeView.Properties;

namespace NeeView
{
    public class OpenOptionsWindowCommand : CommandElement
    {
        public OpenOptionsWindowCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.OpenSettingWindow();
        }
    }
}
