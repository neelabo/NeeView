using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameTransform : IPageFrameTransform, INotifyPropertyChanged, INotifyTransformChanged
    {
        private double _angle;
        private double _scale = 1.0;
        private Point _point;
        public bool _isFlipHorizontal;
        public bool _isFlipVertical;
        private readonly MultiTransform _transform;


        public PageFrameTransform()
        {
            _transform = new MultiTransform();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event TransformChangedEventHandler? TransformChanged;


        public double Scale => _scale;
        public double Angle => _angle;
        public Point Point => _point;
        public bool IsFlipHorizontal => _isFlipHorizontal;
        public bool IsFlipVertical => _isFlipVertical;

        public ITransform NormalTransform => _transform.NormalTransform;
        public ITransform AnimatableTransform => _transform.AnimatableTransform;

        // Content用
        public Transform Transform => _transform.Transform;
        public Transform TransformView => _transform.TransformView;


        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            if (SetProperty(ref _scale, value, nameof(Scale)))
            {
                _transform.SetScale(_scale, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Scale, trigger));
            }
        }

        public void SetAngle(double value, TimeSpan span)
        {
            if (SetProperty(ref _angle, value, nameof(Angle)))
            {
                _transform.SetAngle(_angle, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Angle));
            }
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (SetProperty(ref _point, value, nameof(Point)))
            {
                _transform.SetPoint(_point, span, easeX, easeY);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Point));
            }
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            if (SetProperty(ref _isFlipHorizontal, value, nameof(IsFlipHorizontal)))
            {
                _transform.SetFlipHorizontal(value, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.FlipHorizontal));
            }
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            if (SetProperty(ref _isFlipVertical, value, nameof(IsFlipVertical)))
            {
                _transform.SetFlipVertical(value, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.FlipVertical));
            }
        }

        public Vector GetVelocity()
        {
            return _transform.GetVelocity();
        }

        public void ResetVelocity()
        {
            _transform.ResetVelocity();
        }

        public void Clear()
        {
            Clear(TransformMask.All);
        }

        public void Clear(TransformMask mask)
        {
            if (mask.HasFlag(TransformMask.Flip))
            {
                SetFlipHorizontal(false, TimeSpan.Zero);
                SetFlipVertical(false, TimeSpan.Zero);
            }
            if (mask.HasFlag(TransformMask.Scale))
            {
                SetScale(1.0, TimeSpan.Zero);
            }
            if (mask.HasFlag(TransformMask.Angle))
            {
                SetAngle(0.0, TimeSpan.Zero);
            }
            if (mask.HasFlag(TransformMask.Point))
            {
                SetPoint(default, TimeSpan.Zero);
            }
        }
    }

}
