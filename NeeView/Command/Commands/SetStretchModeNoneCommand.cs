using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeNoneCommand : CommandElement
    {
        public SetStretchModeNoneCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ImageScale");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.None);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewPropertyControl.SetStretchMode(PageStretchMode.None, false, true);
        }
    }
}
