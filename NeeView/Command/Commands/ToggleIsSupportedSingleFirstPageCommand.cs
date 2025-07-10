using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleFirstPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleFirstPageCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(Config.Current.BookSetting.IsSupportedSingleFirstPage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.BookSetting.IsSupportedSingleFirstPage ? TextResources.GetString("ToggleIsSupportedSingleFirstPageCommand.Off") : TextResources.GetString("ToggleIsSupportedSingleFirstPageCommand.On");
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
                Config.Current.BookSetting.IsSupportedSingleFirstPage = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.BookSetting.IsSupportedSingleFirstPage = !Config.Current.BookSetting.IsSupportedSingleFirstPage;
            }
        }
    }
}
