using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class TogglePermitFileCommand : CommandElement
    {
        public TogglePermitFileCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SystemConfig.IsFileWriteAccessEnabled)) { Source = Config.Current.System, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.System.IsFileWriteAccessEnabled);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.System.IsFileWriteAccessEnabled);
            Config.Current.System.IsFileWriteAccessEnabled = state;
        }
    }
}
