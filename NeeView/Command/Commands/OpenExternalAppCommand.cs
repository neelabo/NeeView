using NeeView.Properties;

namespace NeeView
{
    public class OpenExternalAppCommand : CommandElement
    {
        public OpenExternalAppCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanOpenFilePlace();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<OpenExternalAppCommandParameter>();
            BookOperation.Current.Control.OpenApplication(parameter, parameter.MultiPagePolicy);
        }
    }

}
