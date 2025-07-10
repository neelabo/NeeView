using NeeView.Properties;
using System.Windows.Data;

namespace NeeView
{
    public class SetPageOrientationHorizontalCommand : CommandElement
    {
        public SetPageOrientationHorizontalCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.PageSetting");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageFrameOrientation(PageFrameOrientation.Horizontal);
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return TextResources.GetString("PageFrameOrientation.Horizontal");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Book.Orientation = PageFrameOrientation.Horizontal;
        }
    }
}
