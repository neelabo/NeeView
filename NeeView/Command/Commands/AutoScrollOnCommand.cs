using NeeView.Properties;


namespace NeeView
{
    public class AutoScrollOnCommand : CommandElement
    {
        public AutoScrollOnCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("MiddleClick");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewAutoScrollControl.SetAutoScrollMode(true);
        }
    }
}
