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

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.Playlist.IsMarked)) { Source = BookOperation.Current.Playlist, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            return toggleMode switch
            {
                ToggleMode.Toggle => BookOperation.Current.Playlist.IsMarked ? TextResources.GetString("TogglePlaylistItemCommand.Off") : TextResources.GetString("TogglePlaylistItemCommand.On"),
                ToggleMode.On => TextResources.GetString("TogglePlaylistItemCommand.On"),
                ToggleMode.Off => TextResources.GetString("TogglePlaylistItemCommand.Off"),
                _ => ""
            };
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.CanMark();
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                BookOperation.Current.Playlist.SetMark(toggleMode == ToggleMode.On);
            }
            else
            {
                BookOperation.Current.Playlist.ToggleMark();
            }
        }
    }
}
