using NeeView.Threading;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// タッチ通常ドラッグ状態
    /// </summary>
    public class TouchInputDrag : TouchInputBase
    {
        private readonly TouchDragManipulation _manipulation;
        private readonly DelayAction _delayAction = new();


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputDrag(TouchInputContext context) : base(context)
        {
            _manipulation = new TouchDragManipulation(context);
        }


        public TouchDragManipulation Manipulation => _manipulation;


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            _manipulation.Initialize();
            _manipulation.Start();
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
        public override void OnStylusDown(object? sender, StylusDownEventArgs e)
        {
            _delayAction.Cancel();
            _context.ViewScrollContext?.CancelScroll();

            _manipulation.Start();
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object? sender, StylusEventArgs e)
        {
            // タッチされなくなったら解除
            if (_context.TouchMap.Count < 1)
            {
                _manipulation.Stop(System.Environment.TickCount);

                var span = _context.ViewScrollContext?.GetScrollSpan() ?? TimeSpan.Zero;
                _delayAction.Request(() => ResetState(), span);
            }
            else
            {
                _manipulation.Start();
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object? sender, StylusEventArgs e)
        {
            _manipulation.Update(sender, e);
        }

    }
}
