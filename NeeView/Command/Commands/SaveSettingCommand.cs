namespace NeeView
{
    public class SaveSettingCommand : CommandElement
    {
        public SaveSettingCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            SaveDataSync.Current.SaveAll(false, false);
        }
    }
}
