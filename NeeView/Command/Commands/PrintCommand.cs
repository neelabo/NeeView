using NeeView.Properties;

namespace NeeView
{
    public class PrintCommand : CommandElement
    {
        public PrintCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+P");
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.PrintController.CanPrint();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.PrintController.Print();
        }
    }
}
