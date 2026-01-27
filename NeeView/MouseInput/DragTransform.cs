using NeeLaboratory;
using NeeView.PageFrames;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// 表示座標系の操作実行
    /// </summary>
    public class DragTransform : IScaleControl, IAngleControl, IPointControl, IFlipControl
    {
        private readonly ContentDragTransformContext _context;

        public DragTransform(ContentDragTransformContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 拡縮スナップ。0で無効
        /// </summary>
        private double SnapScale { get; set; } = 0;

        public ContentDragTransformContext Context => _context;

        public bool IsFlipHorizontal => _context.Transform.IsFlipHorizontal;
        public bool IsFlipVertical => _context.Transform.IsFlipVertical;
        public double Scale => _context.Transform.Scale;
        public double Angle => _context.Transform.Angle;
        public Point Point => _context.Transform.Point;


        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.Transform.SetPoint(value, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.Transform.AddPoint(value, span, easeX, easeY);
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            _context.Transform.SetScale(value, span, trigger);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _context.Transform.SetAngle(value, span);
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            _context.Transform.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            _context.Transform.SetFlipVertical(value, span);
        }

        /// <summary>
        /// 座標変更
        /// </summary>
        /// <param name="point">新しい座標</param>
        /// <param name="span">変化時間</param>
        public void DoPoint(Point point, TimeSpan span)
        {
            _context.Transform.SetPoint(point, span);
        }

        /// <summary>
        /// 座標加算
        /// </summary>
        /// <param name="delta">変化量</param>
        /// <param name="span">変化時間</param>
        public void DoMove(Vector delta, TimeSpan span)
        {
            _context.Transform.AddPoint(delta, span);
        }

        public void DoMove(Vector delta, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.Transform.AddPoint(delta, span, easeX, easeY);
        }

        /// <summary>
        /// 慣性移動
        /// </summary>
        /// <param name="velocity">速度</param>
        /// <param name="acceleration">加速度(<0)</param>
        public void DoInertia(Vector velocity, double acceleration)
        {
            _context.Transform.InertiaPoint(velocity, acceleration);
        }

        /// <summary>
        /// 慣性計算初期化
        /// </summary>
        public void ResetInertia()
        {
            _context.Transform.ResetInertia();
        }

        /// <summary>
        /// スケール変更
        /// </summary>
        /// <param name="scaleType">変更するスケールの種類</param>
        /// <param name="scale">新しいスケール</param>
        /// <param name="span">変化時間</param>
        /// <param name="withTransform">座標も更新する</param>
        public void DoScale(ScaleType scaleType, double scale, TimeSpan span, bool withTransform = true)
        {
            switch (scaleType)
            {
                case ScaleType.TransformScale:
                    DoScale(scale, span, withTransform);
                    break;
                case ScaleType.BaseScale:
                    DoBaseScale(scale, span, withTransform);
                    break;
                default:
                    throw new ArgumentException("Not support ScaleType", nameof(scaleType));
            }
        }

        /// <summary>
        /// スケール変更
        /// </summary>
        /// <param name="scale">新しいスケール</param>
        /// <param name="span">変化時間</param>
        /// <param name="withTransform">座標も更新する</param>
        public void DoScale(double scale, TimeSpan span, bool withTransform = true)
        {
            scale = GetSnapScale(scale);

            _context.Transform.SetScale(scale, span);

            if (withTransform)
            {
                var scaleRate = scale / _context.StartScale;
                SetScaleTransform(scaleRate, span);
            }
        }

        /// <summary>
        /// 基底スケール変更
        /// </summary>
        /// <param name="scale">新しいスケール</param>
        /// <param name="span">時間変化</param>
        /// <param name="withTransform">座標も更新する</param>
        public void DoBaseScale(double scale, TimeSpan span, bool withTransform = true)
        {
            scale = GetSnapScale(scale);

            Config.Current.BookSetting.BaseScale = scale;

            if (withTransform)
            {
                var scaleRate = scale / _context.StartBaseScale;
                SetScaleTransform(scaleRate, span);
            }
        }

        /// <summary>
        /// スケール値をスナップ
        /// </summary>
        /// <param name="scale">入力スケール値</param>
        /// <returns>スナップされたスケール値</returns>
        private double GetSnapScale(double scale)
        {
            return SnapScale > 0
                ? Math.Floor((scale + SnapScale * 0.5) / SnapScale) * SnapScale
                : scale;
        }

        /// <summary>
        /// スケールによる座標変更を反映
        /// </summary>
        /// <param name="scaleRate">スケール変化割合</param>
        /// <param name="span">時間変化</param>
        private void SetScaleTransform(double scaleRate, TimeSpan span)
        {
            var v0 = _context.ContentCenter - _context.ScaleCenter;
            var v1 = v0 * scaleRate;
            var delta = v1 - v0;
            var pos = _context.StartPoint + delta;
            _context.Transform.SetPoint(pos, span);
        }

        /// <summary>
        /// 角度変更
        /// </summary>
        /// <param name="angle">新しい角度</param>
        /// <param name="span">変化時間</param>
        public void DoRotate(double angle, TimeSpan span)
        {
            var angleFrequency = Config.Current.View.AngleFrequency;
            if (angleFrequency > 0)
            {
                angle = Math.Floor((angle + angleFrequency * 0.5) / angleFrequency) * angleFrequency;
            }

            // 回転
            _context.Transform.SetAngle(angle, span);

            // 回転に伴う移動
            var m = new RotateTransform(_context.Transform.Angle - _context.StartAngle);
            var p0 = _context.ContentCenter;
            var p1 = _context.RotateCenter + (Vector)m.Transform(p0 - (Vector)_context.RotateCenter);
            var delta = p1 - p0;
            var pos = _context.StartPoint + delta;
            _context.Transform.SetPoint(pos, span);
        }

        // 反転実行
        public void DoFlipHorizontal(bool isFlip, TimeSpan span)
        {
            if (_context.Transform.IsFlipHorizontal != isFlip)
            {
                _context.Transform.SetFlipHorizontal(isFlip, span);

                if (_context.StartFlipHorizontal != isFlip)
                {
                    // 角度を反転
                    var angle = -MathUtility.NormalizeLoopRange(_context.StartAngle, -180, 180);
                    _context.Transform.SetAngle(angle, span);

                    // 座標を反転
                    var v0 = _context.ContentCenter - _context.FlipCenter;
                    var v1 = new Vector(-v0.X, v0.Y);
                    var delta = v1 - v0;
                    _context.Transform.SetPoint(_context.StartPoint + delta, span);
                }
                else
                {
                    _context.Transform.SetAngle(_context.StartAngle, span);
                    _context.Transform.SetPoint(_context.StartPoint, span);
                }
            }
        }

        // 反転実行
        public void DoFlipVertical(bool isFlip, TimeSpan span)
        {
            if (_context.Transform.IsFlipVertical != isFlip)
            {
                _context.Transform.SetFlipVertical(isFlip, span);

                if (_context.StartFlipVertical != isFlip)
                {
                    // 角度を反転
                    var angle = 90 - MathUtility.NormalizeLoopRange(_context.Transform.Angle + 90, -180, 180);
                    _context.Transform.SetAngle(angle, span);

                    // 座標を反転
                    var v0 = _context.ContentCenter - _context.FlipCenter;
                    var v1 = new Vector(v0.X, -v0.Y);
                    var delta = v1 - v0;
                    _context.Transform.SetPoint(_context.StartPoint + delta, span);
                }
                else
                {
                    _context.Transform.SetAngle(_context.StartAngle, span);
                    _context.Transform.SetPoint(_context.StartPoint, span);
                }
            }
        }

    }
}
