using NeeView.Maths;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public class ViewTransformControl : ITransformControl, IRevisePositionDelta
    {
        private readonly PageFrameContext _context;
        private readonly ViewTransformContext _viewContext;
        private readonly PageFrameContainer _container;
        private readonly ScrollLock _scrollLock;

        public ViewTransformControl(PageFrameContext context, PageFrameContainer container, ViewTransformContext viewContext, ScrollLock scrollLock)
        {
            _context = context;
            _viewContext = viewContext;
            _container = container;
            _scrollLock = scrollLock;
        }

        public double Scale => _container.Transform.Scale;
        public double Angle => _container.Transform.Angle;
        public Point Point => _viewContext.Point;
        public bool IsFlipHorizontal => _container.Transform.IsFlipHorizontal;
        public bool IsFlipVertical => _container.Transform.IsFlipVertical;


        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            _container.Transform.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            _container.Transform.SetFlipVertical(value, span);
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            _container.Transform.SetScale(value, span, trigger);
            _scrollLock.Unlock();
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _container.Transform.SetAngle(value, span);
            _scrollLock.Unlock();
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.IsSnapAnchor.Reset();
            _viewContext.SetPoint(value, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.IsSnapAnchor.Reset();
            var delta = RevisePositionDelta(value);
            _viewContext.AddPoint(delta, span, easeX, easeY);
        }

        public void InertiaPoint(Vector velocity, double acceleration)
        {
            //var velocity = _viewContext.GetVelocity();

            _context.IsSnapAnchor.Reset();
            var inertiaEaseFactory = new InertiaEaseFactory(GetScrollLockHit, GetAreaLimitHit);
            var multiEaseSet = inertiaEaseFactory.Create(_container.Transform.Point, velocity, acceleration);
            if (!multiEaseSet.IsValid) return;
            _viewContext.AddPoint(multiEaseSet.Delta, TimeSpan.FromMilliseconds(multiEaseSet.Milliseconds), multiEaseSet.EaseX, multiEaseSet.EaseY, true);
        }

        public void ResetInertia()
        {
            //_viewContext.ResetVelocity();
        }

        // 範囲内になるよう移動量補正
        public Vector RevisePositionDelta(Vector delta)
        {
            var canvasRect = _viewContext.CanvasRect;

            if (_context.ViewConfig.IsLimitMove)
            {
                // scroll lock
                _scrollLock.Update(canvasRect, _viewContext.ViewRect);
                delta = _scrollLock.Limit(delta);

                // scroll area limit
                var areaLimit = new ScrollAreaLimit(canvasRect, _viewContext.ViewRect);
                delta = areaLimit.GetLimitContentMove(delta);
            }

            return delta;
        }


        private HitData GetScrollLockHit(Point start, Vector delta)
        {
            if (!_context.ViewConfig.IsLimitMove) return new HitData(start, delta);

            var canvasRect = _viewContext.CanvasRect;
            _scrollLock.Update(canvasRect, _viewContext.ViewRect);
            return _scrollLock.HitTest(start, delta);
        }

        private HitData GetAreaLimitHit(Point start, Vector delta)
        {
            if (!_context.ViewConfig.IsLimitMove) return new HitData(start, delta);

            var canvasRect = _viewContext.CanvasRect;

            // 配置方向の幅を制限なしにする
            if (_context.FrameOrientation == PageFrameOrientation.Horizontal)
            {
                canvasRect.X = double.NegativeInfinity;
                canvasRect.Width = double.PositiveInfinity;
            }
            else
            {
                canvasRect.Y = double.NegativeInfinity;
                canvasRect.Height = double.PositiveInfinity;
            }

            var areaLimit = new ScrollAreaLimit(canvasRect, _viewContext.ViewRect);
            return areaLimit.HitTest(start, delta);
        }

        public void SnapView()
        {
            //throw new NotImplementedException();
        }
    }


    public interface IRevisePositionDelta
    {
        Vector RevisePositionDelta(Vector delta);
    }
}
