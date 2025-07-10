using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetPageModeOneCommand : CommandElement
    {
        public SetPageModeOneCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.ShortCutKey = new ShortcutKey("Ctrl+1");
            this.MouseGesture = new MouseSequence("RU");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.SinglePage);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetPageMode(PageMode.SinglePage);
        }
    }
}
