using NeeView.Properties;

namespace NeeView
{
    public class ToggleSortModeCommand : CommandElement
    {
        public ToggleSortModeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageOrder");
            this.IsShowMessage = true;
        }
        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookOperation.Current.BookControl.PageSortModeClass.GetTogglePageSortMode(BookSettings.Current.SortMode).ToAliasName();
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.ToggleSortMode(BookOperation.Current.BookControl.PageSortModeClass);
        }
    }
}
