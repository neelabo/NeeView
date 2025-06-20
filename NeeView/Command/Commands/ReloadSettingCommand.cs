namespace NeeView
{
    public class ReloadSettingCommand : CommandElement
    {
        public ReloadSettingCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var setting = SaveData.Current.LoadUserSetting(false);
            SaveData.Current.SetUserSettingFileStamp(setting.FileStamp);
            UserSettingTools.Restore(setting);
        }
    }
}
