using System;
using System.Diagnostics;
using System.Windows.Input;

namespace NeeView
{
    public class LoupeDragAction : DragAction
    {
        public LoupeDragAction()
        {
            DragKey = new DragKey("RightButton");
            DragActionCategory = DragActionCategory.None;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            Debug.Assert(context is LoupeDragTransformContext);
            return new ActionControl(context, this);
        }


        private class ActionControl : DragActionControl
        {
            private LoupeDragTransformContext _context;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _context = context as LoupeDragTransformContext ?? throw new ArgumentException("need LoupeDragTransformContext", nameof(context));
            }

            public override void ExecuteBegin()
            {
            }

            public override void Execute()
            {
                _context.Update();
            }

            public override void ExecuteEnd(ISpeedometer? speedometer, bool continued)
            {
            }

            public override void MouseWheel(MouseWheelEventArgs e)
            {
                // TODO: Delta量に応じた回数
                if (e.Delta > 0)
                {
                    _context.ZoomIn();
                }
                else
                {
                    _context.ZoomOut();
                }
                e.Handled = true;
            }
        }
    }
}
