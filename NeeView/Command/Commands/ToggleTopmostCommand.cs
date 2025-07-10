using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleTopmostCommand : CommandElement
    {
        public ToggleTopmostCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowConfig.IsTopmost)) { Source = Config.Current.Window, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Window.IsTopmost ? TextResources.GetString("ToggleTopmostCommand.Off") : TextResources.GetString("ToggleTopmostCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.ToggleTopmost(sender);
        }
    }
}
