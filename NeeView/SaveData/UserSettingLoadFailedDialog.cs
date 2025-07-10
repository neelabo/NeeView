namespace NeeView
{
    /// <summary>
    /// UserSetting 専用 LoadFailedDialog
    /// </summary>
    public class UserSettingLoadFailedDialog : LoadFailedDialog
    {
        public UserSettingLoadFailedDialog(bool cancellable) : base("Notice.LoadSettingFailed", "Notice.LoadSettingFailedTitle")
        {
            OKCommand = new UICommand("Notice.LoadSettingFailedButtonContinue") { IsPossible = true };
            if (cancellable)
            {
                CancelCommand = new UICommand("Notice.LoadSettingFailedButtonQuit") { Alignment = UICommandAlignment.Left };
            }
        }
    }
}
