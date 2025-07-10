using NeeView.Properties;

namespace NeeView
{
    public class OpenContextMenuCommand : CommandElement
    {
        public OpenContextMenuCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.RaiseOpenContextMenuRequest();
        }
    }
}
