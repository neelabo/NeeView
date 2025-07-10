using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundCheckCommand : CommandElement
    {
        public SetBackgroundCheckCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.Check);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.Check;
        }
    }
}
