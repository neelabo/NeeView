using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBookReadOrderLeftCommand : CommandElement
    {
        public SetBookReadOrderLeftCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetBookReadOrder(PageReadOrder.LeftToRight);
        }
    }
}
