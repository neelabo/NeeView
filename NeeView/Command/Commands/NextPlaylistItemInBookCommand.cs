using NeeView.Properties;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NextPlaylistItemInBookCommand : CommandElement
    {
        public NextPlaylistItemInBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = false;

            // PrevPlaylistItemInBook
            this.ParameterSource = new CommandParameterSource(new MovePlaylistItemInBookCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.CanNextMarkInPlace(e.Parameter.Cast<MovePlaylistItemInBookCommandParameter>());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Playlist.NextMarkInPlace(sender, e.Parameter.Cast<MovePlaylistItemInBookCommandParameter>());
        }
    }

}
