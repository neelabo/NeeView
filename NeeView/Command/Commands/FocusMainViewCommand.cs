using System.Runtime.Serialization;

namespace NeeView
{
    public class FocusMainViewCommand : CommandElement
    {
        public FocusMainViewCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Panel");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new FocusMainViewCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewManager.Current.FocusMainView(e.Parameter.Cast<FocusMainViewCommandParameter>());
        }
    }

}
