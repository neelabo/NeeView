using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundAutoCommand : CommandElement
    {
        public SetBackgroundAutoCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.Auto);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.Auto;
        }
    }
}
