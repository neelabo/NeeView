using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedWidePageCommand : CommandElement
    {
        public ToggleIsSupportedWidePageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsSupportedWidePage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsSupportedWidePage ? TextResources.GetString("ToggleIsSupportedWidePageCommand.Off") : TextResources.GetString("ToggleIsSupportedWidePageCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanPageSizeSubSetting(2);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettings.Current.SetIsSupportedWidePage(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                BookSettings.Current.ToggleIsSupportedWidePage();
            }
        }
    }
}
