using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class TogglePlaylistItemCommand : CommandElement
    {
        public TogglePlaylistItemCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Playlist");
            this.ShortCutKey = new ShortcutKey("Ctrl+M");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.Playlist.IsMarked)) { Source = BookOperation.Current.Playlist, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.IsMarked ? TextResources.GetString("TogglePlaylistItemCommand.Off") : TextResources.GetString("TogglePlaylistItemCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.CanMark();
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookOperation.Current.Playlist.SetMark(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                BookOperation.Current.Playlist.ToggleMark();
            }
        }
    }
}
