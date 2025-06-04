namespace NeeView
{
    public class NextPlaylistCommand : CommandElement
    {
        private readonly int _offset = +1;

        public NextPlaylistCommand() : this(+1)
        {
        }

        protected NextPlaylistCommand(int offset)
        {
            _offset = offset;

            this.Group = Properties.TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var path = PlaylistHub.Current.GetNextPlaylist(_offset);
            return ResourceService.GetString("@Word.Playlist") + " - " + LoosePath.GetFileNameWithoutExtension(path);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var path = PlaylistHub.Current.GetNextPlaylist(_offset);
            return !string.IsNullOrEmpty(path);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var path = PlaylistHub.Current.GetNextPlaylist(_offset);
            if (string.IsNullOrEmpty(path)) return;

            PlaylistHub.Current.SelectedItem = path;
        }
    }
}
