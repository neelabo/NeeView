//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// メインビューカーソル管理
    /// </summary>
    /// <remarks>
    /// 一定時間操作がないときに非表示にする
    /// </remarks>
    public class MainViewCursor : IDisposable, ICursorSetter
    {
        private readonly MouseConfig _mouseConfig;
        private readonly FrameworkElement _view;
        private DispatcherTimer? _nonActiveTimer;
        private int _lastActionTime;
        private Point _lastActionPoint;
        private double _cursorMoveDistance;
        private bool _isVisible = true;
        private Cursor? _cursor;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;


        public MainViewCursor(FrameworkElement view)
        {
            _mouseConfig = Config.Current.Mouse;
            _view = view;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <remarks>
        /// ウィンドウの状態によるのでコンストラクタとは初期化タイミングをずらしている
        /// </remarks>
        public void Initialize()
        {
            _view.PreviewMouseMove += MainView_PreviewMouseMove;
            _disposables.Add(() => _view.PreviewMouseMove -= MainView_PreviewMouseMove);
            _view.PreviewMouseDown += MainView_PreviewMouseAction;
            _disposables.Add(() => _view.PreviewMouseDown -= MainView_PreviewMouseAction);
            _view.PreviewMouseUp += MainView_PreviewMouseAction;
            _disposables.Add(() => _view.PreviewMouseUp -= MainView_PreviewMouseAction);
            _view.MouseEnter += MainView_MouseEnter;
            _disposables.Add(() => _view.MouseEnter -= MainView_MouseEnter);

            _nonActiveTimer = new DispatcherTimer(DispatcherPriority.Normal, _view.Dispatcher);
            _nonActiveTimer.Interval = TimeSpan.FromSeconds(0.2);
            _nonActiveTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            _disposables.Add(() => _nonActiveTimer.Stop());

            _disposables.Add(_mouseConfig.SubscribePropertyChanged(nameof(MouseConfig.IsCursorHideEnabled), (s, e) => UpdateNonActiveTimerActivity()));
            UpdateNonActiveTimerActivity();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// カーソル設定
        /// </summary>
        /// <param name="cursor"></param>
        public void SetCursor(Cursor? cursor)
        {
            if (_cursor != cursor)
            {
                _cursor = cursor;
                ResetCursorVisible();
            }
        }

        /// <summary>
        /// カーソル自動非表示設定変更処理
        /// </summary>
        private void UpdateNonActiveTimerActivity()
        {
            if (_nonActiveTimer is null) return;

            if (_mouseConfig.IsCursorHideEnabled)
            {
                _nonActiveTimer.Start();
            }
            else
            {
                _nonActiveTimer.Stop();
            }

            ResetCursorVisible();
        }

        /// <summary>
        /// タイマーイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (_isVisible)
            {
                // 表示時、一定時間経過後に非表示にする処理
                if (!IsVisibleTime() && !MouseButtonBitsExtensions.Create().Any())
                {
                    SetCursorVisible(false);
                }
            }
        }

        private bool IsVisibleTime()
        {
            return System.Environment.TickCount - _lastActionTime <= _mouseConfig.CursorHideTime * 1000;
        }

        /// <summary>
        /// マウス移動イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_PreviewMouseMove(object? sender, MouseEventArgs e)
        {
            var nowPoint = e.GetPosition(_view);
            var delta = Math.Abs(nowPoint.X - _lastActionPoint.X) + Math.Abs(nowPoint.Y - _lastActionPoint.Y);
            _lastActionPoint = nowPoint;

            if (_isVisible)
            {
                // 表示時、移動でタイマーリセット
                if (delta > 0)
                {
                    _lastActionTime = System.Environment.TickCount;
                }
            }
            else
            {
                // 非表示時、一定距離移動で再表示
                _cursorMoveDistance += delta;
                if (IsVisibleDistance())
                {
                    SetCursorVisible(true);
                }
            }
        }

        private bool IsVisibleDistance()
        {
            return _cursorMoveDistance > _mouseConfig.CursorHideReleaseDistance;
        }

        /// <summary>
        /// マウスボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_PreviewMouseAction(object? sender, MouseEventArgs e)
        {
            if (_mouseConfig.IsCursorHideReleaseAction)
            {
                ResetCursorVisible();
            }

            _lastActionTime = System.Environment.TickCount;
        }

        /// <summary>
        /// マウスエンターイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_MouseEnter(object? sender, MouseEventArgs e)
        {
            ResetCursorVisible();
        }

        /// <summary>
        /// マウスカーソル状態リセット
        /// </summary>
        public void ResetCursorVisible()
        {
            _lastActionTime = System.Environment.TickCount;
            _cursorMoveDistance = 0.0;
            SetCursorVisible(true);
        }

        /// <summary>
        /// マウスカーソル表示ON/OFF
        /// </summary>
        /// <remarks>
        /// ビューのカーソル変更をここで行っている
        /// </remarks>
        /// <param name="isVisible"></param>
        private void SetCursorVisible(bool isVisible)
        {
            if (_isVisible != isVisible)
            {
                _isVisible = isVisible;
                if (isVisible)
                {
                    _lastActionTime = System.Environment.TickCount;
                }
                else
                {
                    _cursorMoveDistance = 0.0;
                }
            }

            _view.Cursor = _isVisible ? _cursor : Cursors.None;
        }

    }
}
