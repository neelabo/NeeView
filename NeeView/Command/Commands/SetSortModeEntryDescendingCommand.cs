using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeEntryDescendingCommand : CommandElement
    {
        public SetSortModeEntryDescendingCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.EntryDescending);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit && BookOperation.Current.BookControl.PageSortModeClass.Contains(PageSortMode.EntryDescending);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetSortMode(PageSortMode.EntryDescending);
        }
    }

}
