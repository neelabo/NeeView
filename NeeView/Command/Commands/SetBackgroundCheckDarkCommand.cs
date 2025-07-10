using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundCheckDarkCommand : CommandElement
    {
        public SetBackgroundCheckDarkCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.CheckDark);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.CheckDark;
        }
    }
}
