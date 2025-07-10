using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeSizeCommand : CommandElement
    {
        public SetSortModeSizeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageOrder");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Size);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetSortMode(PageSortMode.Size);
        }
    }
}
