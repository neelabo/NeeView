using NeeView.Maths;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{

    // TODO: ページ移動とPointの初期化問題
    public class ContentTransformControl : ITransformControl, IRevisePositionDelta
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainer _container;
        private readonly Rect _containerRect;
        private readonly ScrollLock _scrollLock;

        public ContentTransformControl(PageFrameContext context, PageFrameContainer container, Rect viewRect, ScrollLock scrollLock)
        {
            _context = context;
            _container = container;
            _containerRect = viewRect;
            _scrollLock = scrollLock;
        }


        public double Scale => _container.Scale;
        public double Angle => _container.Angle;
        public Point Point => _container.Point;
        public bool IsFlipHorizontal => _container.IsFlipHorizontal;
        public bool IsFlipVertical => _container.IsFlipVertical;


        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            _container.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            _container.SetFlipVertical(value, span);
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            _container.SetScale(value, span, trigger);
            _scrollLock.Unlock();
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _container.SetAngle(value, span);
            _scrollLock.Unlock();
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.IsSnapAnchor.Reset();
            _container.SetPoint(value, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.IsSnapAnchor.Reset();
            var delta = RevisePositionDelta(value);
            _container.SetPoint(_container.Point + delta, span, easeX, easeY);
        }

        public void InertiaPoint(Vector velocity, double acceleration)
        {
            //var velocity = _container.Transform.GetVelocity();

            _context.IsSnapAnchor.Reset();
            var inertiaEaseFactory = new InertiaEaseFactory(GetScrollLockHit, GetAreaLimitHit);
            var multiEaseSet = inertiaEaseFactory.Create(_container.Point, velocity, acceleration);
            if (!multiEaseSet.IsValid) return;
            _container.AddPoint(multiEaseSet.Delta, TimeSpan.FromMilliseconds(multiEaseSet.Milliseconds), multiEaseSet.EaseX, multiEaseSet.EaseY, true);
        }

        public void ResetInertia()
        {
            //_container.Transform.ResetVelocity();
        }

        // 範囲内になるよう移動量補正
        public Vector RevisePositionDelta(Vector delta)
        {
            var contentRect = _container.GetContentRect();

            if (_context.ViewConfig.IsLimitMove)
            {
                // scroll lock
                _scrollLock.Update(contentRect, _containerRect);
                delta = _scrollLock.Limit(delta);

                // scroll area limit
                var areaLimit = new ScrollAreaLimit(contentRect, _containerRect);
                delta = areaLimit.GetLimitContentMove(delta);
            }

            return delta;
        }


        private HitData GetScrollLockHit(Point start, Vector delta)
        {
            if (!_context.ViewConfig.IsLimitMove) return new HitData(start, delta);

            var contentRect = _container.GetContentRect(start);
            _scrollLock.Update(contentRect, _containerRect);
            return _scrollLock.HitTest(start, delta);
        }

        private HitData GetAreaLimitHit(Point start, Vector delta)
        {
            if (!_context.ViewConfig.IsLimitMove) return new HitData(start, delta);

            var contentRect = _container.GetContentRect(start);
            var areaLimit = new ScrollAreaLimit(contentRect, _containerRect);
            return areaLimit.HitTest(start, delta);
        }


        public void SnapView()
        {
            //if (!Config.Current.View.IsLimitMove) return;

            var contentRect = _container.GetContentRect();

            var areaLimit = new ScrollAreaLimit(contentRect, _containerRect);
            _container.SetPoint(areaLimit.SnapView(false), TimeSpan.Zero);
        }
    }
}
