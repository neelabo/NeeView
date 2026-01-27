using NeeView.Properties;
using System;
using System.Windows;

namespace NeeView
{
    public class FlipHorizontalDragAction : DragAction
    {
        public FlipHorizontalDragAction()
        {
            Note = TextResources.GetString("DragActionType.FlipHorizontal");
            DragKey = new DragKey("Alt+LeftButton");
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
                DragFlipHorizontal(TimeSpan.Zero);
            }

            // 左右反転
            public void DragFlipHorizontal(TimeSpan span)
            {
                const double margin = 16;

                if (Context.First.X + margin < Context.Last.X)
                {
                    _transformControl.DoFlipHorizontal(true, span);
                    Context.First = new Point(Context.Last.X - margin, Context.First.Y);
                }
                else if (Context.First.X - margin > Context.Last.X)
                {
                    _transformControl.DoFlipHorizontal(false, span);
                    Context.First = new Point(Context.Last.X + margin, Context.First.Y);
                }
            }

        }
    }
}
