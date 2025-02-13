namespace NeeView
{
    public class PrevPlaylistItemCommand : CommandElement
    {
        public PrevPlaylistItemCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading && PlaylistPresenter.Current?.PlaylistListBox?.CanMovePrevious() == true;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var isSuccess = PlaylistPresenter.Current?.PlaylistListBox?.MovePrevious();
            if (isSuccess != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Command, Properties.TextResources.GetString("Notice.PlaylistItemPrevFailed"));
            }
        }
    }
}
