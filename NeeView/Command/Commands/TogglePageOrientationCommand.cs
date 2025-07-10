using NeeView.Properties;

namespace NeeView
{
    public class TogglePageOrientationCommand : CommandElement
    {
        public TogglePageOrientationCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Book.Orientation.GetToggle().ToAliasName();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Book.Orientation = Config.Current.Book.Orientation.GetToggle();
        }
    }
}
