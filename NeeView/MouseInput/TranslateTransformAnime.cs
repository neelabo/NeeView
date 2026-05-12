using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// 座標変更のアニメーション方式を管理
    /// </summary>
    public class TranslateTransformAnime
    {
        private readonly Dictionary<TranslateTransformEasing, IDragTransformPositionState> _map;
        private IDragTransformPositionState _state;

        public TranslateTransformAnime(TranslateTransform translateTransform)
        {
            if (translateTransform is null) throw new ArgumentNullException(nameof(translateTransform));

            _map = new Dictionary<TranslateTransformEasing, IDragTransformPositionState>()
            {
                [TranslateTransformEasing.Direct] = new DirectPositionState(translateTransform),
                [TranslateTransformEasing.Animation] = new AnimatedPositionState(translateTransform),
                [TranslateTransformEasing.Smooth] = new SmoothPositionState(translateTransform),
            };

            _state = _map[TranslateTransformEasing.Direct];
        }

        public void SetPosition(Point point, TranslateTransformEasing easing, TimeSpan span)
        {
            var next = _map[easing];
            if (_state != next)
            {
                _state.Stop();
                _state = next;
            }
            _state.Start(point, span);
        }


        private interface IDragTransformPositionState
        {
            void Start(Point point, TimeSpan span);
            void Stop();
        }

        private class DirectPositionState : IDragTransformPositionState
        {
            private readonly TranslateTransform _translateTransform;

            public DirectPositionState(TranslateTransform translateTransform)
            {
                _translateTransform = translateTransform ?? throw new ArgumentNullException(nameof(translateTransform));
            }

            public void Start(Point value, TimeSpan span)
            {
                _translateTransform.SetPoint(value);
            }

            public void Stop()
            {
            }
        }

        private class AnimatedPositionState : IDragTransformPositionState
        {
            private readonly TranslateTransform _translateTransform;
            private bool _isEnabled;

            public AnimatedPositionState(TranslateTransform translateTransform)
            {
                _translateTransform = translateTransform ?? throw new ArgumentNullException(nameof(translateTransform));
            }

            public void Start(Point point, TimeSpan span)
            {
                _isEnabled = true;

                _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                    DecorateDoubleAnimation(new DoubleAnimation(point.X, span)), HandoffBehavior.Compose);
                _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                    DecorateDoubleAnimation(new DoubleAnimation(point.Y, span)), HandoffBehavior.Compose);

                static DoubleAnimation DecorateDoubleAnimation(DoubleAnimation source)
                {
                    source.AccelerationRatio = 0.4;
                    source.DecelerationRatio = 0.4;
                    return source;
                }
            }

            public void Stop()
            {
                if (_isEnabled)
                {
                    // 解除
                    _translateTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);
                    _translateTransform.ApplyAnimationClock(TranslateTransform.YProperty, null);
                    _isEnabled = false;
                }
            }
        }

        private class SmoothPositionState : IDragTransformPositionState
        {
            private readonly TranslateTransform _translateTransform;
            private bool _isEnabled;
            private Point _point;

            public SmoothPositionState(TranslateTransform translateTransform)
            {
                _translateTransform = translateTransform ?? throw new ArgumentNullException(nameof(translateTransform));
            }

            public void Start(Point point, TimeSpan span)
            {
                _point = point;

                if (!_isEnabled)
                {
                    _isEnabled = true;
                    CompositionTarget.Rendering += OnRendering;
                }
            }

            public void Stop()
            {
                if (_isEnabled)
                {
                    _isEnabled = false;
                    CompositionTarget.Rendering -= OnRendering;
                }
            }

            private void OnRendering(object? sender, EventArgs e)
            {
                if (AppState.Current.IsSuspended)
                {
                    return;
                }

                TickSmooth();
            }

            private void TickSmooth()
            {
                var now = _translateTransform.GetPoint();
                var next = VectorExtensions.Lerp(now, _point, 0.1);

                if ((_point - next).LengthSquared < 1.0)
                {
                    next = _point;
                    Stop();
                }

                _translateTransform.SetPoint(next);
            }
        }
    }
}
