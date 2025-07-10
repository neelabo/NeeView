using NeeView.Properties;

namespace NeeView
{
    public class OpenSettingFilesFolderCommand : CommandElement
    {
        public OpenSettingFilesFolderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.OpenSettingFilesFolder();
        }
    }
}
