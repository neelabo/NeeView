using NeeView.Properties;

namespace NeeView
{
    public class MoveToChildBookCommand : CommandElement
    {
        public MoveToChildBookCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = new ShortcutKey("Alt+Down");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.CanMoveToChildBook();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.MoveToChildBook(this);
        }
    }
}
