using NeeView.Threading;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスドラッグ互換
    /// </summary>
    public class TouchInputMouseDrag : TouchInputBase
    {
        private readonly IDragTransformControl _drag;
        private readonly DelayAction _delayAction = new();
        private TouchContext? _touch;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputMouseDrag(TouchInputContext context) : base(context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (context.DragTransformControl is null) throw new ArgumentException("context.DragTransformControl must not be null.");

            _drag = context.DragTransformControl;
        }


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            _touch = parameter as TouchContext ?? throw new InvalidOperationException("parameter must be TouchContext");

            _drag.ResetState();
            _drag.UpdateState(MouseButtonBits.LeftButton, Keyboard.Modifiers, ToDragCoord(_touch.StartPoint), _touch.StartTimestamp, _context.Speedometer, DragActionUpdateOptions.None);
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            _delayAction.Cancel();
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            // マルチタッチでドラッグへ
            if (_context.TouchMap.Count >= 2)
            {
                SetState(TouchInputState.Drag, null);
            }
            else
            {
                _delayAction.Cancel();
                _context.ViewScrollContext?.CancelScroll();

                _context.TouchMap.TryGetValue(e.StylusDevice, out _touch);
                if (_touch == null) return;

                UpdateDragState(MouseButtonBits.LeftButton, e, DragActionUpdateOptions.None);
            }
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            UpdateDragState(MouseButtonBits.None, e, DragActionUpdateOptions.None);

            var span = _context.ViewScrollContext?.GetScrollSpan() ?? TimeSpan.Zero;
            _delayAction.Request(() => SetState(TouchInputState.Normal, null), span);
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (e.StylusDevice != _touch?.StylusDevice) return;
            UpdateDragState(MouseButtonBits.LeftButton, e, DragActionUpdateOptions.None);
        }

        private void UpdateDragState(MouseButtonBits button, StylusEventArgs e, DragActionUpdateOptions options)
        {
            _drag.UpdateState(button, Keyboard.Modifiers, ToDragCoord(e.GetPosition(_context.Sender)), e.Timestamp, _context.Speedometer, options);
        }
    }

}
