using NeeLaboratory;
using NeeView;
using System;
using System.Windows;

namespace NeeView
{
    public class HoverDragAction : DragAction
    {
        public HoverDragAction()
        {
            DragKey = new DragKey("MiddleButton");
            DragActionCategory = DragActionCategory.Point;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }



        private class ActionControl : NormalDragActionControl
        {
            private MouseConfig _mouseConfig;
            private Point _basePoint;


            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _mouseConfig = Config.Current.Mouse;

                // TODO: ViewTransform か ContentTransform かの区別はこれでいいのか？
                if (Context.Transform is PageFrames.ViewTransformControl)
                {
                    _basePoint = Context.Transform.Point - (Vector)Context.ContentCenter;
                }
                else
                {
                    _basePoint = default;
                }
            }


            public override void Execute()
            {
                Context.UpdateRect();
                HoverScroll(Context.Last, TimeSpan.FromSeconds(_mouseConfig.HoverScrollDuration));
            }

            public override void Flush()
            {
                Context.UpdateRect();
                HoverScroll(Context.Last, TimeSpan.Zero);
            }

            /// <summary>
            /// Hover scroll
            /// </summary>
            /// <param name="point">point in sender</param>
            /// <param name="span">scroll time</param>
            private void HoverScroll(Point point, TimeSpan span)
            {
                var rate = _mouseConfig.HoverScrollSensitivity * -1.0;
                var rateX = point.X / Context.ViewRect.Width * rate;
                var rateY = point.Y / Context.ViewRect.Height * rate;
                HoverScroll(rateX, rateY, span);
            }

            /// <summary>
            /// Hover scroll
            /// </summary>
            /// <param name="rateX">point.X rate in sender [-1.0, 1.0]</param>
            /// <param name="rateY">point.Y rate in sender [-1.0, 1.0]</param>
            /// <param name="span">scroll time</param>
            private void HoverScroll(double rateX, double rateY, TimeSpan span)
            {
                // TODO: StaticFrame のみ？
                // ブラウザのようなスクロール(AutoScroll)は別機能

                var x = Math.Max(Context.ContentRect.Width - Context.ViewRect.Width, 0.0) * rateX.Clamp(-0.5, 0.5);
                var y = Math.Max(Context.ContentRect.Height - Context.ViewRect.Height, 0.0) * rateY.Clamp(-0.5, 0.5);
                var pos = new Point(_basePoint.X + x, _basePoint.Y + y);

                Context.Transform.SetPoint(pos, span);
            }
        }
    }
}
