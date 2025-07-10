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
            return BookHub.Current.IsBookLocked ? TextResources.GetString("ToggleBookLockCommand.Off") : TextResources.GetString("ToggleBookLockCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookHub.Current.IsBookLocked = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                BookHub.Current.IsBookLocked = !BookHub.Current.IsBookLocked;
            }
        }
    }
}
