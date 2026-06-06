using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleBookmarkCommand : CommandElement
    {
        public ToggleBookmarkCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Bookmark");
            this.ShortCutKey = new ShortcutKey("Ctrl+D");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleBookmarkCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.BookControl.IsBookmark)) { Source = BookOperation.Current.BookControl, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            return toggleMode switch
            {
                ToggleMode.Toggle => BookOperation.Current.BookControl.IsBookmarkOn(GetFolderPath(e)) ? TextResources.GetString("ToggleBookmarkCommand.Off") : TextResources.GetString("ToggleBookmarkCommand.On"),
                ToggleMode.On => TextResources.GetString("ToggleBookmarkCommand.On"),
                ToggleMode.Off => TextResources.GetString("ToggleBookmarkCommand.Off"),
                _ => ""
            };
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanBookmark();
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                BookOperation.Current.BookControl.SetBookmark(toggleMode == ToggleMode.On, GetFolderPath(e));
            }
            else
            {
                BookOperation.Current.BookControl.ToggleBookmark(GetFolderPath(e));
            }
        }

        private string? GetFolderPath(CommandContext e)
        {
            return e.Parameter.Cast<ToggleBookmarkCommandParameter>().Folder;
        }
    }
}
