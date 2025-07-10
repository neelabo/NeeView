using NeeView.Properties;
using System;

namespace NeeView
{
    public class LoadAsCommand : CommandElement
    {
        public LoadAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+O");
            this.IsShowMessage = false;
        }

        [MethodArgument]
        public override void Execute(object? sender, CommandContext e)
        {
            var path = e.Args.Length > 0 ? e.Args[0] as string : null;
            if (string.IsNullOrWhiteSpace(path))
            {
                MainWindowModel.Current.LoadAs();
            }
            else
            {
                MainWindowModel.Current.LoadAs(path);
            }
        }
    }
}
