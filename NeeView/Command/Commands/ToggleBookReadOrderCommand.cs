using NeeView.Properties;

namespace NeeView
{
    public class ToggleBookReadOrderCommand : CommandElement
    {
        public ToggleBookReadOrderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.BookReadOrder.GetToggle().ToAliasName();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.ToggleBookReadOrder();
        }
    }
}
