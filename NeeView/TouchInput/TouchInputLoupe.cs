using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチルーペ
    /// </summary>
    public class TouchInputLoupe : TouchInputBase
    {
        private readonly LoupeContext _loupe;
        private TouchContext? _touch;
        private TouchDragContext? _origin;
        private double _originScale;
        private LoupeDragTransformContext? _transformContext;


        public TouchInputLoupe(TouchInputContext context) : base(context)
        {
            if (context.Loupe is null) throw new InvalidOperationException();
            _loupe = context.Loupe;
        }


        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter">TouchContext</param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            _touch = parameter as TouchContext ?? throw new InvalidOperationException("parameter must be TouchContext");

            AttachLoupe(sender);

            sender.Focus();
            SetCursor(Cursors.None);

            _loupe.IsEnabled = true;

            if (Config.Current.Loupe.IsResetByRestart)
            {
                _loupe.Reset();
            }
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
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
        }

        /// <summary>
        /// ルーペ解除
        /// </summary>
        /// <param name="sender"></param>
        private void DetachLoupe(FrameworkElement sender)
        {
            _transformContext?.DetachLoupeContext();
            _transformContext = null;
        }

        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);
            _originScale = _loupe.Scale;
        }

        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (e.Handled) return;
            if (_touch is null) return;

            if (!_context.TouchMap.ContainsKey(_touch.StylusDevice))
            {
                ResetState();
            }
        }

        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (e.Handled) return;
            if (_touch is null) return;
            if (_transformContext is null) return;

            if (e.StylusDevice == _touch.StylusDevice)
            {
                var point = ToDragCoord(e.GetPosition(_context.Sender));

                _transformContext.Update(point, e.Timestamp, DragActionUpdateOptions.None);
                _transformContext.Update();
            }

            if (_origin is not null && _context.TouchMap.Count >= 2)
            {
                var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

                var scale = current.Radius / _origin.Radius;
                _loupe.Scale = _originScale * scale;
            }

            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Config.Current.Loupe.IsWheelScalingEnabled)
            {
                if (e.Delta > 0)
                {
                    _loupe.ZoomIn();
                }
                else
                {
                    _loupe.ZoomOut();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        public override void OnKeyDown(object sender, KeyEventArgs e)
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
