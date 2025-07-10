using NeeView.Properties;

namespace NeeView
{
    public class MoveToParentBookCommand : CommandElement
    {
        public MoveToParentBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = new ShortcutKey("Alt+Up");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookHub.Current.CanLoadParent();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookHub.Current.RequestLoadParent(this);
        }
    }
}
