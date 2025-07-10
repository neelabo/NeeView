using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleMainViewFloatingCommand : CommandElement
    {
        public ToggleMainViewFloatingCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.IsShowMessage = false;
            this.ShortCutKey = new ShortcutKey("F12");
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainViewConfig.IsFloating)) { Source = Config.Current.MainView, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.MainView.IsFloating ? TextResources.GetString("ToggleMainViewFloatingCommand.Off") : TextResources.GetString("ToggleMainViewFloatingCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.MainView.IsFloating = !Config.Current.MainView.IsFloating;
        }
    }
}
