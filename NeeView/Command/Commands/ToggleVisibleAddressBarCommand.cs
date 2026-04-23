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
            return Config.Current.MenuBar.IsAddressBarEnabled ? TextResources.GetString("ToggleVisibleAddressBarCommand.Off") : TextResources.GetString("ToggleVisibleAddressBarCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Options.HasFlag(CommandOption.ByMenu))
            {
                Config.Current.MenuBar.IsAddressBarEnabled = e.Args.Length > 0
                    ? Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture)
                    : !Config.Current.MenuBar.IsAddressBarEnabled;
            }
            else
            {
                if (e.Args.Length > 0)
                {
                    var isVisible = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
                    MainWindowModel.Current.SetAddressBarVisible(isVisible.ToVisibilityRequest());
                }
                else
                {
                    MainWindowModel.Current.SetAddressBarVisible(VisibilityRequest.Toggle);
                }
            }
        }
    }
}
