using NeeView.Properties;

namespace NeeView
{
    public class TogglePageModeCommand : CommandElement
    {

        public TogglePageModeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new TogglePageModeCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.PageMode.GetToggle(+1, e.Parameter.Cast<TogglePageModeCommandParameter>().IsLoop).ToAliasName();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.TogglePageMode(+1, e.Parameter.Cast<TogglePageModeCommandParameter>().IsLoop);
        }
    }
}
