using NeeView.Properties;

namespace NeeView
{
    public class SetDefaultPageSettingCommand : CommandElement
    {
        public SetDefaultPageSettingCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.BookSettingDefault.CopyTo(Config.Current.BookSetting);
        }
    }
}
