using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeTimeStampCommand : CommandElement
    {
        public SetSortModeTimeStampCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.TimeStamp);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetSortMode(PageSortMode.TimeStamp);
        }
    }
}
