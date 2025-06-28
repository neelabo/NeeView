//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウス入力状態
    /// </summary>
    public enum MouseInputState
    {
        None,
        Normal,
        Loupe,
        Drag,
        Gesture,
        AutoScroll
    }

    /// <summary>
    /// MouseInputManager
    /// </summary>
    [LocalDebug]
    public partial class MouseInput : BindableBase
    {
        /// <summary>
        /// マウスクリックによるコマンド発動を抑制する
        /// </summary>
        /// <remarks>
        /// ArchivePage でダブルクリック移動できるようにするために使用されます。
        /// </remarks>
        public static bool IgnoreMouseCommand { get; set; }
        private static int _serialCount;

        private readonly int _serialNumber = _serialCount++;
        private readonly FrameworkElement _sender;
        private MouseInputState _state;
        private MouseHorizontalWheelSource? _mouseHorizontalWheelSource;

        /// <summary>
        /// 現在状態（実体）
        /// </summary>
        private MouseInputBase? _current;

        /// <summary>
        /// 遷移テーブル
        /// </summary>
        private readonly Dictionary<MouseInputState, MouseInputBase?> _mouseInputCollection;

        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        private readonly MouseInputContext _context;

        /// <summary>
        /// マウス移動検知用
        /// </summary>
        private Point _lastActionPoint;


        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseInput(MouseInputContext context)
        {
            _context = context;
            _sender = _context.Sender;

            this.Normal = new MouseInputNormal(_context);
            this.Normal.StateChanged += StateChanged;
            this.Normal.MouseButtonChanged += Mouse_MouseButtonChanged;
            this.Normal.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
            this.Normal.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);

            if (_context.DragTransformContextFactory != null)
            {
                this.Loupe = new MouseInputLoupe(_context);
                this.Loupe.StateChanged += StateChanged;
                this.Loupe.MouseButtonChanged += Mouse_MouseButtonChanged;
                this.Loupe.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
                this.Loupe.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
            }

            if (_context.DragTransformControl != null)
            {
                this.Drag = new MouseInputDrag(_context);
                this.Drag.StateChanged += StateChanged;
                this.Drag.MouseButtonChanged += Mouse_MouseButtonChanged;
                this.Drag.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
                this.Drag.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
            }

            if (_context.IsGestureEnabled)
            {
                this.Gesture = new MouseInputGesture(_context);
                this.Gesture.StateChanged += StateChanged;
                this.Gesture.MouseButtonChanged += Mouse_MouseButtonChanged;
                this.Gesture.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
                this.Gesture.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
                this.Gesture.GestureChanged += (s, e) => _context.GestureCommandCollection?.Execute(e.Sequence);
                this.Gesture.GestureProgressed += (s, e) => _context.GestureCommandCollection?.ShowProgressed(e.Sequence);
            }

            if (_context.DragTransformContextFactory != null)
            {
                this.AutoScroll = new MouseInputAutoScroll(_context);
                this.AutoScroll.StateChanged += StateChanged;
                this.AutoScroll.MouseButtonChanged += Mouse_MouseButtonChanged;
                this.AutoScroll.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
                this.AutoScroll.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(_sender, e);
            }

            // initialize state
            _mouseInputCollection = new Dictionary<MouseInputState, MouseInputBase?>
            {
                { MouseInputState.Normal, this.Normal },
                { MouseInputState.Loupe, this.Loupe },
                { MouseInputState.Drag, this.Drag },
                { MouseInputState.Gesture, this.Gesture },
                { MouseInputState.AutoScroll, this.AutoScroll }
            };
            SetState(MouseInputState.Normal, null);

            // initialize event
            // NOTE: 時々操作が奪われしてまう原因の可能性その１
            _sender.MouseDown += OnMouseButtonDown;
            _sender.MouseUp += OnMouseButtonUp;
            _sender.MouseWheel += OnMouseWheel;
            _sender.MouseMove += OnMouseMove;
            _sender.PreviewKeyDown += OnKeyDown;

            // 水平ホイールイベント管理
            _sender.Loaded += (s, e) => InitializeMouseHorizontalWheel();
            _sender.Unloaded += (s, e) => ReleaseMouseHorizontalWheel();
            InitializeMouseHorizontalWheel();

            // ルーペモード監視
            _context.Loupe?.SubscribePropertyChanged(nameof(LoupeContext.IsEnabled), LoupeContext_IsEnabledChanged);

            // ブック変更監視
            _context.NotifyPageFrameBoxChanged?.SubscribePageFrameBoxChanged(OnPageFrameBoxChanged);
        }



        /// <summary>
        /// ボタン入力イベント
        /// </summary>
        [Subscribable]
        public event EventHandler<MouseButtonEventArgs>? MouseButtonChanged;

        /// <summary>
        /// ホイール入力イベント
        /// </summary>
        [Subscribable]
        public event EventHandler<MouseWheelEventArgs>? MouseWheelChanged;

        /// <summary>
        /// 水平ホイール入力イベント
        /// </summary>
        [Subscribable]
        public event EventHandler<MouseWheelEventArgs>? MouseHorizontalWheelChanged;

        /// <summary>
        /// 一定距離カーソルが移動したイベント
        /// </summary>
        [Subscribable]
        public event EventHandler<MouseEventArgs>? MouseMoved;


        /// <summary>
        /// 状態：既定
        /// </summary>
        public MouseInputNormal Normal { get; private set; }

        /// <summary>
        /// 状態：ルーペ
        /// </summary>
        public MouseInputLoupe? Loupe { get; private set; }

        /// <summary>
        /// 状態：ドラッグ
        /// </summary>
        public MouseInputDrag? Drag { get; private set; }

        /// <summary>
        /// 状態：ジェスチャー
        /// </summary>
        public MouseInputGesture? Gesture { get; private set; }

        /// <summary>
        /// 状態：オートスクロール
        /// </summary>
        public MouseInputAutoScroll? AutoScroll { get; private set; }

        /// <summary>
        /// 現在の状態
        /// </summary>
        public MouseInputState State => _state;


        /// <summary>
        /// マウスクリック
        /// </summary>
        /// <remarks>
        /// コマンド発動用
        /// </remarks>
        private void Mouse_MouseButtonChanged(object? sender, MouseButtonEventArgs e)
        {
            if (IgnoreMouseCommand) return;
            MouseButtonChanged?.Invoke(_sender, e);
        }


        private void LoupeContext_IsEnabledChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_state == MouseInputState.Loupe && (_context.Loupe is null || !_context.Loupe.IsEnabled))
            {
                SetState(MouseInputState.Normal, null);
            }
        }

        /// <summary>
        /// コマンド系イベントクリア
        /// </summary>
        public void ClearMouseEventHandler()
        {
            MouseButtonChanged = null;
            MouseWheelChanged = null;
            MouseHorizontalWheelChanged = null;
        }

        /// <summary>
        /// 水平ホイール有効化
        /// </summary>
        private void InitializeMouseHorizontalWheel()
        {
            _mouseHorizontalWheelSource?.Dispose();

            if (Window.GetWindow(_sender) is not INotifyMouseHorizontalWheelChanged window) return;

            ////Debug.WriteLine($"MouseInput.Sender: InitializeMouseHorizontalWheel()");
            _mouseHorizontalWheelSource = new MouseHorizontalWheelSource(_sender, window);
            _mouseHorizontalWheelSource.MouseHorizontalWheelChanged += (s, e) => OnMouseHorizontalWheel(s, e);
        }

        /// <summary>
        /// 水平ホイール無効化
        /// </summary>
        private void ReleaseMouseHorizontalWheel()
        {
            ////Debug.WriteLine($"MouseInput.Sender: ReleaseMouseHorizontalWheel()");
            _mouseHorizontalWheelSource?.Dispose();
            _mouseHorizontalWheelSource = null;
        }

        public bool IsCaptured()
        {
            return _current?.IsCaptured() == true;
        }

        /// <summary>
        /// 状態変更イベント処理
        /// </summary>
        private void StateChanged(object? sender, MouseInputStateEventArgs e)
        {
            SetState(e.State, e.Parameter);
        }


        /// <summary>
        /// 状態変更
        /// </summary>
        public void SetState(MouseInputState state, object? parameter, bool force = false)
        {
            if (!force && state == _state) return;
            //Debug.WriteLine($"#MouseState: {state}");

            var inputOld = _current;
            var inputNew = _mouseInputCollection[state];

            if (inputNew is null)
            {
                //Debug.WriteLine($"MouseInput: Not support state: {inputNew}");
                return;
            }

            _state = state;
            _current = inputNew;
            inputOld?.OnClosed(_sender);
            inputNew?.OnOpened(_sender, parameter);

            // NOTE: MouseCaptureの影響で同じUIスレッドで再入する可能性があるため、まとめて処理
            inputOld?.OnCaptureClosed(_sender);
            inputNew?.OnCaptureOpened(_sender);
        }

        /// <summary>
        /// 状態初期化
        /// </summary>
        public void ResetState()
        {
            SetState(MouseInputState.Normal, null, true);
        }

        private static bool IsStylusDevice(MouseEventArgs e)
        {
            return e.StylusDevice != null && Config.Current.Touch.IsEnabled;
        }

        private bool IsMouseButtonEnabled(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !_context.IsLeftButtonDownEnabled)
            {
                return false;
            }
            if (e.ChangedButton == MouseButton.Right && !_context.IsRightButtonDownEnabled)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// OnMouseButtonDown
        /// </summary>
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            LocalWritePoint("Down", e);

            bool isEnabled = (sender == _sender)
                && !IsStylusDevice(e)
                && IsMouseButtonEnabled(e)
                && !MouseActivateService.Current.IsMouseActivate;

            if (isEnabled)
            {
                _context.Speedometer.Reset();
                _context.Speedometer.Add(e.GetPosition(_sender), e.Timestamp);

                _current?.OnMouseButtonDown(_sender, e);
            }

            if (_context.IsMouseEventTerminated)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// OnMouseButtonUp
        /// </summary>
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            LocalWritePoint("Up", e);

            bool isEnabled = (sender == _sender)
                && !IsStylusDevice(e)
                && IsMouseButtonEnabled(e);

            if (isEnabled)
            {
                LocalWriteLine($"Up.Time: {System.Environment.TickCount}");
                _context.Speedometer.Add(e.GetPosition(_sender), e.Timestamp);

                _current?.OnMouseButtonUp(_sender, e);
            }

            // 右クリックでのコンテキストメニュー無効
            if (_context.IsMouseEventTerminated)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// OnMouseWheel
        /// </summary>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool isEnabled = (sender == _sender)
                && !IsStylusDevice(e)
                && _context.IsVerticalWheelEnabled;

            if (isEnabled)
            {
                _current?.OnMouseWheel(_sender, e);
            }

            if (_context.IsMouseEventTerminated)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// マウス水平ホイール
        /// </summary>
        private void OnMouseHorizontalWheel(object sender, MouseWheelEventArgs e)
        {
            bool isEnabled = (sender == _sender)
                && _context.IsHorizontalWheelEnabled;

            if (isEnabled)
            {
                ////Debug.WriteLine($"MouseInput: OnMouseHorizontalWheel: {e.Delta} ({e.Timestamp})");
                _current?.OnMouseHorizontalWheel(_sender, e);
            }

            if (_context.IsMouseEventTerminated)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// OnMouseMove
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var buttons = MouseButtonBitsExtensions.Create(e);
            if (buttons.Any())
            {
                LocalWritePoint("Move", e);
            }

            bool isEnabled = (sender == _sender)
                && !IsStylusDevice(e);

            if (isEnabled)
            {
                if (buttons.Any())
                {
                    _context.Speedometer.Add(e.GetPosition(_sender), e.Timestamp);
                }

                _current?.OnMouseMove(_sender, e);

                // マウス移動を通知
                var nowPoint = e.GetPosition(_sender);
                if (Math.Abs(nowPoint.X - _lastActionPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(nowPoint.Y - _lastActionPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    MouseMoved?.Invoke(this, e);
                    _lastActionPoint = nowPoint;
                }

                // ##
                //DevTextMap.Current.SetText("Mouse", $"{nowPoint}");
            }
        }

        /// <summary>
        /// OnKeyDown
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            bool isEnabled = (sender == _sender);

            if (isEnabled)
            {
                _current?.OnKeyDown(_sender, e);
            }
        }

        /// <summary>
        /// Book changed
        /// </summary>
        private void OnPageFrameBoxChanged(object? sender, PageFrameBoxChangedEventArgs e)
        {
            _current?.OnPageFrameBoxChanged(_sender);
        }

        /// <summary>
        /// Cancel input
        /// </summary>
        public void Cancel()
        {
            _current?.Cancel();
        }


        public void UpdateSelectedFrame(FrameChangeType changeType)
        {
            //Debug.WriteLine($"UpdateSelectedFrame: {changeType}");
            _current?.OnUpdateSelectedFrame(changeType);
        }

        [Conditional("LOCAL_DEBUG")]
        private void LocalWriteLine(string s, params object[] args)
        {
            LocalDebug.WriteLine($"({_serialNumber}): " + s);
        }

        [Conditional("LOCAL_DEBUG")]
        private void LocalWritePoint(string label, MouseEventArgs e)
        {
            var buttons = MouseButtonBitsExtensions.Create(e);
            var isStylus = e.StylusDevice != null;
            LocalWriteLine($"{label}: {isStylus} {e.GetPosition(_sender):f0}, {e.Timestamp}, {buttons}");
        }

    }
}
