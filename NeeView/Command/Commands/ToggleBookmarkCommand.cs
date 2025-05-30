using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleBookmarkCommand : CommandElement
    {
        public ToggleBookmarkCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Bookmark");
            this.ShortCutKey = new ShortcutKey("Ctrl+D");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleBookmarkCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.BookControl.IsBookmark)) { Source = BookOperation.Current.BookControl, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.IsBookmarkOn(GetFolderPath(e)) ? Properties.TextResources.GetString("ToggleBookmarkCommand.Off") : Properties.TextResources.GetString("ToggleBookmarkCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.CanBookmark();
        }

        [MethodArgument("@ToggleBookmarkCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookOperation.Current.BookControl.SetBookmark(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), GetFolderPath(e));
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
