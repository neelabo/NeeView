using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{


    /// <summary>
    /// マウス通常入力状態
    /// </summary>
    public class MouseInputNormal : MouseInputBase
    {
        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isButtonDown;

        /// <summary>
        /// 長押し判定用タイマー
        /// </summary>
        private readonly Timer _timer;

        private MouseButtonEventArgs? _mouseButtonEventArgs;

        private HoverTransformControl? _hoverTransformControl;

        private MouseWheelScroll? _wheelScroll;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputNormal(MouseInputContext context) : base(context)
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimeout;

            if (_context.DragTransformContextFactory is not null)
            {
                _hoverTransformControl = new HoverTransformControl(_context.DragTransformContextFactory);
                _wheelScroll = new(_context.DragTransformContextFactory);
            }
        }

        private bool IsLongButtonPressed()
        {
            return Config.Current.Mouse.LongButtonDownMode != LongButtonDownMode.None
                && (CreateMouseButtonBits() & Config.Current.Mouse.LongButtonMask.ToMouseButtonBits()) != 0;
        }

        private void StartTimer()
        {
            _timer.Interval = Config.Current.Mouse.LongButtonDownTime * 1000.0;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
        }

        private void OnTimeout(object? sender, object e)
        {
            switch (Config.Current.Mouse.LongButtonDownMode)
            {
                case LongButtonDownMode.Loupe:
                    StopTimer();
                    AppDispatcher.Invoke(() =>
                    {
                        if (CheckLongButtonUp()) return;
                        SetState(MouseInputState.Loupe, true);
                    });
                    break;

                case LongButtonDownMode.AutoScroll:
                    StopTimer();
                    AppDispatcher.Invoke(() =>
                    {
                        if (CheckLongButtonUp()) return;
                        SetState(MouseInputState.AutoScroll, true);
                    });
                    break;

                case LongButtonDownMode.Repeat:
                    var interval = Config.Current.Mouse.LongButtonRepeatTime * 1000.0;
                    if (_timer.Interval != interval)
                    {
                        _timer.Interval = interval;
                    }
                    AppDispatcher.Invoke(() =>
                    {
                        if (CheckLongButtonUp()) return;
                        if (_mouseButtonEventArgs != null)
                        {
                            MouseButtonChanged?.Invoke(sender, _mouseButtonEventArgs);
                        }
                        _isButtonDown = false;
                    });
                    break;

                default:
                    StopTimer();
                    break;
            }

            bool CheckLongButtonUp()
            {
                if (IsLongButtonPressed()) return false;

                _isButtonDown = false;
                StopTimer();
                return true;
            }
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            _isButtonDown = false;
            SetCursor(null);

            OnUpdateSelectedFrame(FrameChangeType.None);
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            Cancel();
        }

        public override bool IsCaptured()
        {
            return _isButtonDown;
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;
            _context.Sender.Focus();

            _context.StartPoint = e.GetPosition(_context.Sender);
            _context.StartTimestamp = e.Timestamp;

            // ダブルクリック？
            if (e.ClickCount >= 2)
            {
                // コマンド決定
                MouseButtonChanged?.Invoke(sender, e);
                if (e.Handled)
                {
                    Cancel();
                    return;
                }
            }

            if (e.StylusDevice == null)
            {
                // 長押し判定開始
                if (IsLongButtonPressed())
                {
                    StartTimer();

                    // リピート用にパラメータ保存
                    _mouseButtonEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
                }
            }
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
        {
            StopTimer();

            if (!_isButtonDown) return;

            // コマンド決定
            // 離されたボタンがメインキー、それ以外は装飾キー
            MouseButtonChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            _wheelScroll?.OnMouseVerticalWheel(sender, e);

            // コマンド決定
            // ホイールがメインキー、それ以外は装飾キー
            if (!e.Handled)
            {
                MouseWheelChanged?.Invoke(sender, e);
            }

            // その後の操作は全て無効
            Cancel();
        }

        /// <summary>
        /// マウス水平ホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseHorizontalWheel(object? sender, MouseWheelEventArgs e)
        {
            _wheelScroll?.OnMouseHorizontalWheel(sender, e);

            // コマンド決定
            // ホイールがメインキー、それ以外は装飾キー
            if (!e.Handled)
            {
                MouseHorizontalWheelChanged?.Invoke(sender, e);
            }

            // その後の操作は全て無効
            Cancel();
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            HoverScrollIfEnabled(sender, e);

            if (!_isButtonDown) return;

            var point = e.GetPosition(_context.Sender);

            var deltaX = Math.Abs(point.X - _context.StartPoint.X);
            var deltaY = Math.Abs(point.Y - _context.StartPoint.Y);

            // drag check
            if (deltaX > Config.Current.Mouse.MinimumDragDistance || deltaY > Config.Current.Mouse.MinimumDragDistance)
            {
                SwitchToDragState(sender, e);
            }
        }

        // ドラッグ開始。処理をドラッグ系に移行
        private void SwitchToDragState(object? sender, MouseEventArgs e)
        {
            var action = DragActionTable.Current.GetActionType(new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers));
            if (string.IsNullOrEmpty(action))
            {
            }
            else if (Config.Current.Mouse.IsGestureEnabled && _context.IsGestureEnabled && action == DragActionTable.GestureDragActionName)
            {
                SetState(MouseInputState.Gesture);
            }
            else if (Config.Current.Mouse.IsDragEnabled)
            {
                SetState(MouseInputState.Drag, e);
            }
        }


        public override void OnUpdateSelectedFrame(FrameChangeType changeType)
        {
            _hoverTransformControl?.UpdateSelected();
        }


        /// <summary>
        /// ホバースクロール
        /// </summary>
        private void HoverScrollIfEnabled(object? sender, MouseEventArgs e)
        {
            if (_hoverTransformControl is null) return;

            _hoverTransformControl.IsEnabled = Config.Current.Mouse.IsHoverScroll;
            if (_hoverTransformControl.IsEnabled)
            {
                var pos = ToDragCoord(e.GetPosition(_context.Sender));
                var timestamp = e.Timestamp;
                _hoverTransformControl.HoverScroll(pos, timestamp);
            }
        }

        /// <summary>
        /// 入力をキャンセル
        /// </summary>
        public override void Cancel()
        {
            _isButtonDown = false;
            StopTimer();
        }

    }
}
