using NeeView.Properties;
using System.Windows.Data;

namespace NeeView
{
    public class SetPageOrientationVerticalCommand : CommandElement
    {
        public SetPageOrientationVerticalCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageFrameOrientation(PageFrameOrientation.Vertical);
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return TextResources.GetString("PageFrameOrientation.Vertical");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Book.Orientation = PageFrameOrientation.Vertical;
        }
    }
}
