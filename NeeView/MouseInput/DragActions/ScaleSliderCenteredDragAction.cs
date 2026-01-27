using NeeView.Properties;

namespace NeeView
{
    public class ScaleSliderCenteredDragAction : DragAction
    {
        public ScaleSliderCenteredDragAction()
        {
            Note = TextResources.GetString("DragActionType.ScaleSliderCentered");
            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ScaleSliderCenteredActionControl(context, this, ScaleType.TransformScale);
        }
    }

}
