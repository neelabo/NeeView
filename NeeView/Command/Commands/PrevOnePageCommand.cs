using NeeView.Properties;

namespace NeeView
{
    public class PrevOnePageCommand : CommandElement
    {
        public PrevOnePageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.MouseGesture = new MouseSequence("LR");
            this.IsShowMessage = false;
            this.PairPartner = "NextOnePage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MovePrevOne(this);
        }
    }
}
