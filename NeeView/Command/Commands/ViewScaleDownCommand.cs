using NeeLaboratory;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScaleDownCommand : CommandElement
    {
        public ViewScaleDownCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("RightButton+WheelDown");
            this.IsShowMessage = false;

            // ViewScaleUp
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScaleDown(e.Parameter.Cast<ViewScaleCommandParameter>());
        }
    }

}
