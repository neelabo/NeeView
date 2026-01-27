using NeeView.Interop;
using NeeView.Windows;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスルーペ
    /// </summary>
    public class MouseInputLoupe : MouseInputBase
    {
        private readonly LoupeContext _loupe;
        private bool _isLongDownMode;
        private bool _isButtonDown;
        private DragActionProxy _action = new DragActionProxy();
        private LoupeDragTransformContext? _transformContext;
        private POINT _nativePoint;
        private POINT _nativeDelta;

        // TODO: LoupeDragAction 操作でなくてもここで LoupeDragTransformControl 直接操作でいけそう？


        public MouseInputLoupe(MouseInputContext context) : base(context)
        {
            if (context.Loupe is null) throw new InvalidOperationException();
            _loupe = context.Loupe;
        }



        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter">trueならば長押しモード</param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            AttachLoupe(sender);

            if (parameter is bool isLongDownMode)
            {
                _isLongDownMode = isLongDownMode;
            }
            else
            {
                _isLongDownMode = false;
            }

            sender.Focus();
            SetCursor(Cursors.None);

            _loupe.IsEnabled = true;
            _isButtonDown = false;

            if (Config.Current.Loupe.IsResetByRestart)
            {
                _loupe.Reset();
            }
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            SetCursor(null);
            _loupe.IsEnabled = false;
            DetachLoupe(sender);
        }

        /// <summary>
        /// ブック変更によるルーペ設定更新
        /// </summary>
        /// <param name="sender"></param>
        public override void OnPageFrameBoxChanged(FrameworkElement sender)
        {
            DetachLoupe(sender);
            AttachLoupe(sender);
        }

        /// <summary>
        /// ルーペ適用
        /// </summary>
        /// <param name="sender"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void AttachLoupe(FrameworkElement sender)
        {
            _transformContext = _context.DragTransformContextFactory?.CreateLoupeDragTransformContext();
            if (_transformContext is null) throw new NotImplementedException(); // TODO: モード拒否

            _transformContext.AttachLoupeContext(_loupe);

            var action = new LoupeDragAction().CreateControl(_transformContext);
            _action.SetAction(action);
            _action.ExecuteBegin(ToDragCoord(Mouse.GetPosition(sender)), System.Environment.TickCount);
        }

        /// <summary>
        /// ルーペ解除
        /// </summary>
        /// <param name="sender"></param>
        private void DetachLoupe(FrameworkElement sender)
        {
            _action.ExecuteEnd(ToDragCoord(Mouse.GetPosition(sender)), System.Environment.TickCount, _context.Speedometer, options: DragActionUpdateOptions.None, continued: false);
            _action.ClearAction();

            _transformContext?.DetachLoupeContext();
            _transformContext = null;
        }

        public override void OnCaptureOpened(FrameworkElement sender)
        {
            _nativePoint = CursorInfo.GetNativeCursorPos();
            _nativeDelta = new();

            MouseInputHelper.CaptureMouse(this, sender);
        }

        public override void OnCaptureClosed(FrameworkElement sender)
        {
            _nativePoint += _nativeDelta;
            _nativeDelta = new();
            CursorInfo.SetNativeCursorPos(_nativePoint);

            MouseInputHelper.ReleaseMouseCapture(this, sender);
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;

            if (_isLongDownMode)
            {
            }
            else
            {
                // ダブルクリック？
                if (e.ClickCount >= 2)
                {
                    // コマンド決定
                    MouseButtonChanged?.Invoke(sender, e);
                    if (e.Handled)
                    {
                        // その後の操作は全て無効
                        _isButtonDown = false;
                    }
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
            if (_isLongDownMode)
            {
                if (MouseButtonBitsExtensions.Create(e) == MouseButtonBits.None)
                {
                    // ルーペ解除
                    ResetState();
                }
            }
            else
            {
                if (!_isButtonDown) return;

                // コマンド決定
                // 離されたボタンがメインキー、それ以外は装飾キー
                MouseButtonChanged?.Invoke(sender, e);

                // その後の入力は全て無効
                _isButtonDown = false;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            _nativeDelta += CursorInfo.GetNativeCursorPos() - _nativePoint;
            CursorInfo.SetNativeCursorPos(_nativePoint);

            var point = CursorInfo.GetPosition(_nativePoint + _nativeDelta, _context.Sender);
            _action.Execute(ToDragCoord(point), e.Timestamp, DragActionUpdateOptions.None);
            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (Config.Current.Loupe.IsWheelScalingEnabled)
            {
                _action.MouseWheel(e);
                e.Handled = true;
            }
            else
            {
                // コマンド決定
                // ホイールがメインキー、それ以外は装飾キー
                MouseWheelChanged?.Invoke(sender, e);

                // その後の操作は全て無効
                _isButtonDown = false;
            }
        }

        /// <summary>
        /// マウス水平ホイール処理
        /// </summary>
        public override void OnMouseHorizontalWheel(object? sender, MouseWheelEventArgs e)
        {
            // コマンド決定
            // ホイールがメインキー、それ以外は装飾キー
            MouseHorizontalWheelChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // ESC で 状態解除
            if (Config.Current.Loupe.IsEscapeKeyEnabled && e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                ResetState();
                e.Handled = true;
            }
        }

        /// <summary>
        /// フレーム変更
        /// </summary>
        /// <param name="changeType"></param>
        public override void OnUpdateSelectedFrame(FrameChangeType changeType)
        {
            if (changeType == FrameChangeType.Range && Config.Current.Loupe.IsResetByPageChanged && !Config.Current.Book.IsPanorama)
            {
                AppDispatcher.BeginInvoke(ResetState);
            }
        }
    }
}
