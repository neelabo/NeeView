using NeeView.Windows.Media;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// オートスクロール
    /// </summary>
    public class MouseInputAutoScroll : MouseInputBase
    {
        private bool _isLongDownMode;
        private Point _start;
        private Point _end;
        private Vector _velocity;
        private DragTransform? _transformControl;
        private IDisposable? _renderingSubscriber;


        public MouseInputAutoScroll(MouseInputContext context) : base(context)
        {
        }


        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter">trueならば長押しモード</param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            if (_context.DragTransformContextFactory is null) throw new InvalidOperationException();

            _isLongDownMode = parameter is bool isLongDownMode && isLongDownMode;

            sender.Focus();
            SetCursor(Cursors.ScrollAll);

            _transformControl = CreateTransformControl();
            _start = Mouse.GetPosition(_context.Sender);
            _end = _start;
            _velocity = default;

            _renderingSubscriber ??= CompositionTargetEx.SubscribeRendering(CompositionTarget_Rendering);
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            SetCursor(null);

            _renderingSubscriber?.Dispose();
            _renderingSubscriber = null;

            _transformControl = null;
        }

        /// <summary>
        /// ブック変更イベント
        /// </summary>
        /// <param name="sender"></param>
        public override void OnPageFrameBoxChanged(FrameworkElement sender)
        {
            ResetState();
        }

        /// <summary>
        /// 現在の DragTransform を作る
        /// </summary>
        /// <returns></returns>
        private DragTransform? CreateTransformControl()
        {
            var transformContext = _context.DragTransformContextFactory?.CreateContentDragTransformContext(false);
            if (transformContext is null) return null;

            return new DragTransform(transformContext);
        }

        /// <summary>
        /// 描写定期処理。オートスクロール処理を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompositionTarget_Rendering(object? sender, RenderingDeltaEventArgs e)
        {
            //NVDebug.WriteInfo("Rendering", $"{(int)e.DeltaTime.TotalMilliseconds} ms");
            _transformControl?.DoMove(-_velocity * e.DeltaTime.TotalMilliseconds, TimeSpan.Zero);
        }

        public override void OnCaptureOpened(FrameworkElement sender)
        {
            MouseInputHelper.CaptureMouse(this, sender);
        }

        public override void OnCaptureClosed(FrameworkElement sender)
        {
            MouseInputHelper.ReleaseMouseCapture(this, sender);
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            ResetState();
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_isLongDownMode)
            {
                ResetState();
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            _end = e.GetPosition(_context.Sender);

            var sensitivity = Config.Current.Mouse.AutoScrollSensitivity;
            double min = sensitivity;
            const double max = 16.0;
            const double scale = 0.02;
            var delta = (_end - _start) * scale * sensitivity;
            _velocity = new Vector(LimitScalarValue(delta.X, min, max), LimitScalarValue(delta.Y, min, max));

            SetCursor(GetScrollCursor(_velocity));
        }

        /// <summary>
        /// スカラー値の制限
        /// </summary>
        /// <param name="value">元の値</param>
        /// <param name="min">この値を0とする</param>
        /// <param name="max">最大値</param>
        /// <returns></returns>
        private static double LimitScalarValue(double value, double min, double max)
        {
            var scalar = Math.Abs(value);
            if (scalar < min) return 0.0;
            var length = Math.Min(scalar - min, max);
            return length * (value < 0.0 ? -1.0 : 1.0);
        }

        /// <summary>
        /// スクロールカーソルを得る
        /// </summary>
        /// <param name="delta">移動方向</param>
        /// <returns>スクロールカーソル</returns>
        private static Cursor GetScrollCursor(Vector delta)
        {
            return delta.X switch
            {
                < 0.0 => delta.Y switch
                {
                    < 0.0 => Cursors.ScrollNW,
                    > 0.0 => Cursors.ScrollSW,
                    _ => Cursors.ScrollW,
                },
                > 0.0 => delta.Y switch
                {
                    < 0.0 => Cursors.ScrollNE,
                    > 0.0 => Cursors.ScrollSE,
                    _ => Cursors.ScrollE,
                },
                _ => delta.Y switch
                {
                    < 0.0 => Cursors.ScrollN,
                    > 0.0 => Cursors.ScrollS,
                    _ => Cursors.ScrollAll,
                },
            };
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            ResetState();
            MouseWheelChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// マウス水平ホイール処理
        /// </summary>
        public override void OnMouseHorizontalWheel(object? sender, MouseWheelEventArgs e)
        {
            ResetState();
            MouseHorizontalWheelChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            ResetState();
        }

        /// <summary>
        /// フレーム変更
        /// </summary>
        /// <param name="changeType"></param>
        public override void OnUpdateSelectedFrame(FrameChangeType changeType)
        {
            if (changeType == FrameChangeType.Range && !Config.Current.Book.IsPanorama)
            {
                // _transformControl 再設定
                _transformControl = CreateTransformControl();
                if (_transformControl is null)
                {
                    ResetState();
                }
            }
        }
    }

}
