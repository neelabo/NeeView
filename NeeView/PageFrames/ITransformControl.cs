using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public interface ITransformControlObject
    {
    }

    public interface IScaleControl : ITransformControlObject
    {
        public double Scale { get; }
        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None);
    }

    public interface IAngleControl : ITransformControlObject
    {
        public double Angle { get; }
        public void SetAngle(double value, TimeSpan span);
    }

    public interface IPointControl : ITransformControlObject
    {
        public Point Point { get; }
        public Point ViewPoint => Point;
        public void SetPoint(Point value, TimeSpan span);
        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY);
        public void AddPoint(Vector value, TimeSpan span)
            => AddPoint(value, span, null, null);
        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
            => SetPoint(Point + value, span, easeX, easeY);
    }

    public interface IFlipControl : ITransformControlObject
    {
        public bool IsFlipHorizontal { get; }
        public bool IsFlipVertical { get; }
        public void SetFlipHorizontal(bool value, TimeSpan span);
        public void SetFlipVertical(bool value, TimeSpan span);
    }


    public interface ITransformControl : IScaleControl, IAngleControl, IPointControl, IFlipControl
    {
        void InertiaPoint(Vector velocity, double acceleration) { }
        void ResetInertia() { }

        void SnapView();
    }
}
