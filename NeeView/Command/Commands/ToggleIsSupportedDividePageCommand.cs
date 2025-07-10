using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedDividePageCommand : CommandElement
    {
        public ToggleIsSupportedDividePageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsSupportedDividePage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsSupportedDividePage ? TextResources.GetString("ToggleIsSupportedDividePageCommand.Off") : TextResources.GetString("ToggleIsSupportedDividePageCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanPageModeSubSetting(PageMode.SinglePage);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettings.Current.SetIsSupportedDividePage(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                BookSettings.Current.ToggleIsSupportedDividePage();
            }
        }
    }
}
