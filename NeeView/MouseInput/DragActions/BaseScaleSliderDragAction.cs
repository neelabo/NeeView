using NeeView.Properties;
using System;

namespace NeeView
{
    public class BaseScaleSliderDragAction : DragAction
    {
        public BaseScaleSliderDragAction()
        {
            Note = TextResources.GetString("DragActionType.BaseScaleSlider");
            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ScaleSliderActionControl(context, this, ScaleType.BaseScale);
        }
    }
}
