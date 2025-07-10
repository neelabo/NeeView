using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsRecursiveFolderCommand : CommandElement
    {
        public ToggleIsRecursiveFolderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsRecursiveFolder));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsRecursiveFolder ? TextResources.GetString("ToggleIsRecursiveFolderCommand.Off") : TextResources.GetString("ToggleIsRecursiveFolderCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettings.Current.SetIsRecursiveFolder(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                BookSettings.Current.ToggleIsRecursiveFolder();
            }
        }
    }
}
