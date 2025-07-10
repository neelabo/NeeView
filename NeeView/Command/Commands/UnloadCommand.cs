using NeeView.Properties;

namespace NeeView
{
    public class UnloadCommand : CommandElement
    {
        public UnloadCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookHub.Current.CanUnload();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookHub.Current.RequestUnload(this, true);
        }
    }
}
