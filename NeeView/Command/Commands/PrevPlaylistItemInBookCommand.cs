using NeeView.Properties;

namespace NeeView
{
    public class PrevPlaylistItemInBookCommand : CommandElement
    {
        public PrevPlaylistItemInBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePlaylistItemInBookCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.CanPrevMarkInPlace(e.Parameter.Cast<MovePlaylistItemInBookCommandParameter>());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Playlist.PrevMarkInPlace(sender, e.Parameter.Cast<MovePlaylistItemInBookCommandParameter>());
        }
    }
}
