using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleBookLockCommand : CommandElement
    {
        public ToggleBookLockCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.BookMove");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookHub.IsBookLocked)) { Source = BookHub.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, BookHub.Current.IsBookLocked);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, BookHub.Current.IsBookLocked);
            BookHub.Current.IsBookLocked = state;
        }
    }
}
