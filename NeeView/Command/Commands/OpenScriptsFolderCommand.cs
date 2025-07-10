using NeeView.Properties;

namespace NeeView
{
    public class OpenScriptsFolderCommand : CommandElement
    {
        public OpenScriptsFolderCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return Config.Current.Script.IsScriptFolderEnabled;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ScriptManager.Current.OpenScriptsFolder();
        }
    }
}
