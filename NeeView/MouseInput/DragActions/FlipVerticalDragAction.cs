using NeeView.Properties;
using System;
using System.Windows;

namespace NeeView
{
    public class FlipVerticalDragAction : DragAction
    {
        public FlipVerticalDragAction()
        {
            Note = TextResources.GetString("DragActionType.FlipVertical");
            //DragKey = new DragKey("Alt+RightButton");
            DragActionCategory = DragActionCategory.Flip;
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
                DragFlipVertical(TimeSpan.Zero);
            }

            // 上下反転
            public void DragFlipVertical(TimeSpan span)
            {
                const double margin = 16;

                if (Context.First.Y + margin < Context.Last.Y)
                {
                    _transformControl.DoFlipVertical(true, span);
                    Context.First = new Point(Context.First.X, Context.Last.Y - margin);
                }
                else if (Context.First.Y - margin > Context.Last.Y)
                {
                    _transformControl.DoFlipVertical(false, span);
                    Context.First = new Point(Context.First.X, Context.Last.Y + margin);
                }
            }

        }
    }
}
