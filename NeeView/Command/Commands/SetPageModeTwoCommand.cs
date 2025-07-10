using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetPageModeTwoCommand : CommandElement
    {
        public SetPageModeTwoCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.ShortCutKey = new ShortcutKey("Ctrl+2");
            this.MouseGesture = new MouseSequence("RD");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.WidePage);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetPageMode(PageMode.WidePage);
        }
    }
}
