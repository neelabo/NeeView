using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleAutoScrollCommand : CommandElement
    {
        public ToggleAutoScrollCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("MiddleClick");
            this.IsShowMessage = false;
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainViewComponent.Current.ViewAutoScrollControl.IsAutoScrollMode)) { Source = MainViewComponent.Current.ViewAutoScrollControl, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewAutoScrollControl.GetAutoScrollMode());
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, MainViewComponent.Current.ViewAutoScrollControl.GetAutoScrollMode());
            MainViewComponent.Current.ViewAutoScrollControl.SetAutoScrollMode(state);
        }
    }
}
