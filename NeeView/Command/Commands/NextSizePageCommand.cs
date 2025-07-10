using NeeLaboratory;
using NeeView.Properties;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NextSizePageCommand : CommandElement
    {
        public NextSizePageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Move");
            this.IsShowMessage = false;
            this.PairPartner = "PrevSizePage";

            // PrevSizePage
            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.MoveNextSize(this, e.Parameter.Cast<MoveSizePageCommandParameter>().Size);
        }
    }

}
