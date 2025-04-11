using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public class AnimatableTransform : ITransform
    {
        private double _scale = 1.0;
        private double _angle;
        private Point _point;
        public bool _isFlipHorizontal;
        public bool _isFlipVertical;

        private readonly ScaleTransform _flipTransform;
        private readonly ScaleTransform _scaleTransform;
        private readonly RotateTransform _rotateTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;


        public AnimatableTransform()
        {
            _flipTransform = new ScaleTransform();
            _scaleTransform = new ScaleTransform();
            _rotateTransform = new RotateTransform();
            _translateTransform = new TranslateTransform();
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_flipTransform);
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_rotateTransform);
            _transformGroup.Children.Add(_translateTransform);
        }

        public Transform Transform => _transformGroup;
        public ScaleTransform FlipTransform => _flipTransform;
        public ScaleTransform ScaleTransform => _scaleTransform;
        public RotateTransform RotateTransform => _rotateTransform;
        public TranslateTransform TranslateTransform => _translateTransform;


        public void SetScale(double value, TimeSpan span)
        {
            _scale = value;
            SetPropertyValue(_scaleTransform, ScaleTransform.ScaleXProperty, value, span);
            SetPropertyValue(_scaleTransform, ScaleTransform.ScaleYProperty, value, span);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _angle = value;
            SetPropertyValue(_rotateTransform, RotateTransform.AngleProperty, value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            var changeX = _point.X != value.X;
            var changeY = _point.Y != value.Y;
            _point = value;

            // 変化がある、もしくはリセットするときにアニメーション適用
            if (changeX || span <= TimeSpan.Zero)
            {
                SetPropertyValue(_translateTransform, TranslateTransform.XProperty, value.X, span, easeX);
            }
            if (changeY || span <= TimeSpan.Zero)
            {
                SetPropertyValue(_translateTransform, TranslateTransform.YProperty, value.Y, span, easeY);
            }
        }

        public void SetFlipHorizontal(bool isFlipHorizontal, TimeSpan span)
        {
            _isFlipHorizontal = isFlipHorizontal;
            SetPropertyValue(_flipTransform, ScaleTransform.ScaleXProperty, isFlipHorizontal ? -1.0 : 1.0, span);
        }

        public void SetFlipVertical(bool isFlipVertical, TimeSpan span)
        {
            _isFlipVertical = isFlipVertical;
            SetPropertyValue(_flipTransform, ScaleTransform.ScaleYProperty, isFlipVertical ? -1.0 : 1.0, span);
        }

        public void Flush()
        {
            SetScale(_scale, TimeSpan.Zero);
            SetAngle(_angle, TimeSpan.Zero);
            SetPoint(_point, TimeSpan.Zero);
            SetFlipHorizontal(_isFlipHorizontal, TimeSpan.Zero);
            SetFlipVertical(_isFlipVertical, TimeSpan.Zero);
        }


        private void SetPropertyValue(Animatable anime, DependencyProperty dp, double value, TimeSpan span, IEasingFunction? ease = null)
        {
            if (span <= TimeSpan.Zero)
            {
                anime.BeginAnimation(dp, null);
                anime.SetValue(dp, value);
            }
            else
            {
                var animation = new DoubleAnimation();
                animation.To = value;
                animation.Duration = new Duration(span);
                // ドラッグ操作のような場合
                animation.EasingFunction = ease ?? EaseTools.DefaultEase;
                // キーボード操作のような連続して発行される場合は以下の方が良い
                //animation.AccelerationRatio = IsScrolling() ? 0.4 : 0.0;
                //animation.DecelerationRatio = 0.4;

                anime.BeginAnimation(dp, animation, HandoffBehavior.Compose);
            }
        }
    }


    public static class EaseTools
    {
        //public static IEasingFunction DefaultEase { get; } = new CubicEase() { EasingMode = EasingMode.EaseOut };
        public static IEasingFunction DefaultEase { get; } = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
        public static LinerEase LinearEase { get; } = new LinerEase();
    }
}
