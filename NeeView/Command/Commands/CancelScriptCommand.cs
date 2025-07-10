using NeeView.Properties;

namespace NeeView
{
    public class CancelScriptCommand : CommandElement
    {
        public CancelScriptCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Script");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ScriptManager.Current.CancelAll();
        }
    }
}
