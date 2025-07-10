using NeeView.Properties;
using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundWhiteCommand : CommandElement
    {
        public SetBackgroundWhiteCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.White);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.White;
        }
    }
}
