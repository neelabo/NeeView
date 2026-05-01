using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleAddressBarCommand : CommandElement
    {
        public ToggleVisibleAddressBarCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MenuBarConfig.IsAddressBarEnabled)) { Source = Config.Current.MenuBar };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.MenuBar.IsAddressBarEnabled, MainWindow.Current.IsAddressBarVisible);
            return GetStateExecuteMessage(state);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.MenuBar.IsAddressBarEnabled, MainWindow.Current.IsAddressBarVisible);
            if (e.ByMenu)
            {
                Config.Current.MenuBar.IsAddressBarEnabled = state;
            }
            else
            {
                MainWindowModel.Current.SetAddressBarVisible(state.ToStateRequest());
            }
        }
    }
}
