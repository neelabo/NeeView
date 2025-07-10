using NeeLaboratory;
using NeeView.Properties;
using System;
using System.Windows;

namespace NeeView
{
    public class AngleSliderDragAction : DragAction
    {
        public AngleSliderDragAction()
        {
            Note = TextResources.GetString("DragActionType.AngleSlider");
            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
            //DragKey = new DragKey("Shift+RightButton");
            DragActionCategory = DragActionCategory.Angle;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }


        private class ActionControl : NormalDragActionControl
        {
            private DragTransform _transformControl;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _transformControl = new DragTransform(Context);
            }

            public override void Execute()
            {
                DragAngleSlider(Context.First, Context.Last, Parameter.Cast<SensitiveDragActionParameter>().Sensitivity, TimeSpan.Zero);
            }

            public void DragAngleSlider(Point start, Point end, double sensitivity, TimeSpan span)
            {
                var angle = MathUtility.NormalizeLoopRange(Context.StartAngle + (start.X - end.X) * 0.5 * sensitivity, -180, 180);
                _transformControl.DoRotate(angle, span);
            }
        }


    }
}
