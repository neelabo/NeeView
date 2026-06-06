using NeeView.Properties;
using NeeView.Windows;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleFullDesktopCommand : CommandElement
    {
        public ToggleFullDesktopCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.ShortCutKey = new ShortcutKey("Shift+F11");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            var windowStateManager = MainWindow.Current.WindowStateManager;
            return new Binding(nameof(windowStateManager.IsFullDesktop)) { Source = windowStateManager, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewWindowControl.IsFullDesktop);
            return GetStateExecuteMessage(state);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !WindowParameters.IsTabletMode;
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewWindowControl.IsFullDesktop);
            MainViewComponent.Current.ViewWindowControl.SetWindowFullDesktop(sender, state);
        }
    }
}
