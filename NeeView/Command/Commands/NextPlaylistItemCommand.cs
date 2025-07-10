using NeeView.Properties;

namespace NeeView
{
    public class NextPlaylistItemCommand : CommandElement
    {
        public NextPlaylistItemCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading && PlaylistPresenter.Current?.PlaylistListBox?.CanMoveNext() == true;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var isSuccess = PlaylistPresenter.Current?.PlaylistListBox?.MoveNext();
            if (isSuccess != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Command, TextResources.GetString("Notice.PlaylistItemNextFailed"));
            }
        }
    }
}
