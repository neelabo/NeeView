using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeEntryCommand : CommandElement
    {
        public SetSortModeEntryCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Entry);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit && BookOperation.Current.BookControl.PageSortModeClass.Contains(PageSortMode.Entry);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetSortMode(PageSortMode.Entry);
        }
    }
}
