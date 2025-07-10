using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleLastPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleLastPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsSupportedSingleLastPage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsSupportedSingleLastPage ? TextResources.GetString("ToggleIsSupportedSingleLastPageCommand.Off") : TextResources.GetString("ToggleIsSupportedSingleLastPageCommand.On");
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
                BookSettings.Current.SetIsSupportedSingleLastPage(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                BookSettings.Current.ToggleIsSupportedSingleLastPage();
            }
        }
    }
}
