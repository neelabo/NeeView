using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    // タッチ処理
    public class TouchInputForGestureEditor : BindableBase
    {
        private readonly TouchInputContext _context;
        private readonly FrameworkElement _sender;

        /// <summary>
        /// 現在状態（実体）
        /// </summary>
        private TouchInputBase? _current;


        public TouchInputForGestureEditor(FrameworkElement sender)
        {
            _context = new TouchInputContext(sender, null, null, null, null, null, null, null);
            _sender = sender;

            this.Gesture = new TouchInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;

            this.Normal = new TouchInputNormal(_context, this.Gesture);
            this.Normal.StateChanged += StateChanged;

            // initialize state
            _touchInputCollection = new Dictionary<TouchInputState, TouchInputBase>
            {
                { TouchInputState.Normal, this.Normal },
                { TouchInputState.Gesture, this.Gesture }
            };
            SetState(TouchInputState.Normal, null);

            // initialize event
            _sender.PreviewStylusDown += OnStylusDown;
            _sender.PreviewStylusUp += OnStylusUp;
            _sender.PreviewStylusMove += OnStylusMove;
        }


        /// <summary>
        /// 状態：既定
        /// </summary>
        public TouchInputNormal Normal { get; private set; }

        /// <summary>
        /// 状態：ジェスチャー
        /// </summary>
        public TouchInputGesture Gesture { get; private set; }

        /// <summary>
        /// 遷移テーブル
        /// </summary>
        private readonly Dictionary<TouchInputState, TouchInputBase> _touchInputCollection;


        /// <summary>
        /// 現在状態
        /// </summary>
        private TouchInputState _state;
        public TouchInputState State => _state;



        //
        public bool IsCaptured()
        {
            return _context.TouchMap.Any();
        }

        /// <summary>
        /// 状態変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateChanged(object? sender, TouchInputStateEventArgs e)
        {
            SetState(e.State, e.Parameter);
        }

        /// <summary>
        /// 状態変更
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parameter"></param>
        public void SetState(TouchInputState state, object? parameter)
        {
            if (state == _state) return;

            // 未定義の状態はキャンセル
            if (!_touchInputCollection.ContainsKey(state)) return;

            _current?.OnClosed(_sender);
            _state = state;
            _current = _touchInputCollection[_state];
            _current.OnOpened(_sender, parameter);
        }

        // 非アクティブなデバイスを削除
        private void CleanupTouchMap()
        {
            _context.TouchMap = _context.TouchMap.Where(item => !item.Key.InAir).ToDictionary(item => item.Key, item => item.Value);
        }

        //
        private void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            if (sender != _sender) return;

            CleanupTouchMap();

            _context.TouchMap[e.StylusDevice] = new TouchContext(e.StylusDevice, e.GetPosition(_sender), e.Timestamp);

            _sender.CaptureStylus();

            _current?.OnStylusDown(_sender, e);
        }

        //
        private void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (sender != _sender) return;

            _context.TouchMap.Remove(e.StylusDevice);

            CleanupTouchMap();

            if (!_context.TouchMap.Any())
            {
                _sender.ReleaseStylusCapture();
            }

            _current?.OnStylusUp(_sender, e);
        }

        //
        private void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (sender != _sender) return;
            _current?.OnStylusMove(_sender, e);
        }

    }
}
