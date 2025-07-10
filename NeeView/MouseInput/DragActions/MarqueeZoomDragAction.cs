using NeeView.Properties;
using System;
using System.Windows;

namespace NeeView
{
    public class MarqueeZoomDragAction : DragAction
    {
        public MarqueeZoomDragAction()
        {
            Note = TextResources.GetString("DragActionType.MarqueeZoom");
            //DragKey = new DragKey("Shift+RightButton");
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }

        private class ActionControl : NormalDragActionControl, IDisposable
        {
            private AreaSelectAdorner _adorner;
            private bool _disposedValue;


            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _adorner = new AreaSelectAdorner(Context.Sender);
            }

            protected override void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        _adorner.Detach();
                    }

                    _disposedValue = true;
                }

                base.Dispose(disposing);
            }

            public override void Execute()
            {
                DragMarqueeZoom(Context.First, Context.Last);
            }

            public override void ExecuteEnd(ISpeedometer? speedometer, bool continued)
            {
                DragMarqueeZoomEnd(Context.First, Context.Last, TimeSpan.Zero);
            }

            private void DragMarqueeZoom(Point start, Point end)
            {
                var coordCenter = new Vector(Context.Sender.ActualWidth * 0.5, Context.Sender.ActualHeight * 0.5);
                _adorner.Start = start + coordCenter;
                _adorner.End = end + coordCenter;
                _adorner.Attach();
            }

            private void DragMarqueeZoomEnd(Point start, Point end, TimeSpan span)
            {
                _adorner.Detach();

                var zoomRect = new Rect(start, end);
                if (zoomRect.Width < 0 || zoomRect.Height < 0) return;

                var zoomX = Context.ViewRect.Width / zoomRect.Width;
                var zoomY = Context.ViewRect.Height / zoomRect.Height;
                var zoom = zoomX < zoomY ? zoomX : zoomY;
                Context.Transform.SetScale(Context.Transform.Scale * zoom, span);

                var p0 = Context.ContentCenter + (zoomRect.TopLeft - Context.ContentCenter) * zoom;
                var p1 = Context.ContentCenter + (zoomRect.BottomRight - Context.ContentCenter) * zoom;
                var vc = ((Vector)p0 + (Vector)p1) * 0.5;
                Context.Transform.SetPoint(Context.StartPoint - vc, span);
            }

        }
    }

}
