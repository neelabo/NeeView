using NeeView.Properties;

namespace NeeView
{
    public class ToggleMediaPlayCommand : CommandElement
    {
        public ToggleMediaPlayCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Video");
        }
        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookOperation.Current.IsMediaPlaying() ? TextResources.GetString("Word.Stop") : TextResources.GetString("Word.Play");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.MediaExists();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.ToggleMediaPlay();
        }
    }
}
