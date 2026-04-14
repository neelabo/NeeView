using NeeView.Properties;

namespace NeeView
{
    public class PrevMediaPositionCommand : CommandElement
    {
        public PrevMediaPositionCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Video");

            this.ParameterSource = new CommandParameterSource(new MoveMediaPositionCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.MediaExists();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var delta = e.Parameter.Cast<MoveMediaPositionCommandParameter>().Delta;
            if (delta == 0.0)
            {
                delta = Config.Current.Archive.Media.PageSeconds;
            }

            BookOperation.Current.MoveMediaPosition(-delta);
        }
    }

}
