using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class LoupeTransformContext : IPointControl, IScaleControl, INotifyPropertyChanged, INotifyTransformChanged
    {
        private PageFrameContext _context;
        private LoupeTransform _transform = new LoupeTransform();


        public LoupeTransformContext(PageFrameContext context)
        {
            _context = context;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;


        public Transform Transform => _transform.Transform;

        public Point Point => _transform.Point;

        public double Scale => _transform.Scale;


        public Transform GetCanvasTransform()
        {
            return _context.IsStaticFrame ? Transform.Identity : _transform.Transform;
        }

        public Transform GetContentTransform()
        {
            return _context.IsStaticFrame ? _transform.Transform : Transform.Identity;
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        // NOTE: no use easing function.
        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (_transform.Point != value)
            {
                _transform.SetPoint(value);
                RaisePropertyChanged(nameof(Point));
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Loupe, TransformAction.Point));
            }
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            SetPoint(_transform.Point + value, span, easeX, easeY);
        }


        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            if (_transform.Scale != value)
            {
                _transform.SetScale(value);
                RaisePropertyChanged(nameof(Scale));
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Loupe, TransformAction.Scale, trigger));
            }
        }
    }
}
