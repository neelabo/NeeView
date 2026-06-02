using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleFullScreenCommand : CommandElement
    {
        public ToggleFullScreenCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.ShortCutKey = new ShortcutKey("F11");
            this.MouseGesture = new MouseSequence("U");
            this.IsShowMessage = false;
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            var windowStateManager = MainWindow.Current.WindowStateManager;
            return new Binding(nameof(windowStateManager.IsFullScreen)) { Source = windowStateManager, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewWindowControl.IsFullScreen);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewWindowControl.IsFullScreen);
            MainViewComponent.Current.ViewWindowControl.SetWindowFullScreen(sender, state);
        }
    }
}
