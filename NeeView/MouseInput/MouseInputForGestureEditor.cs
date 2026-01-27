using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// MouseInputForGestureEditor for GestureEditor
    /// </summary>
    public class MouseInputForGestureEditor
    {
        /// <summary>
        /// 状態：既定
        /// </summary>
        public MouseInputNormalForGestureEdit Normal { get; private set; }

        /// <summary>
        /// 状態：ジェスチャー
        /// </summary>
        public MouseInputGesture Gesture { get; private set; }

        /// <summary>
        /// 状態テーブル
        /// </summary>
        private readonly Dictionary<MouseInputState, MouseInputBase> _mouseInputCollection;

        /// <summary>
        /// 現在状態
        /// </summary>
        public MouseInputState _state;

        /// <summary>
        /// 現在状態（実体）
        /// </summary>
        private MouseInputBase? _current;

        /// <summary>
        /// 入力ターゲット
        /// </summary>
        private readonly FrameworkElement _sender;

        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        private readonly MouseInputContext _context;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="sender"></param>
        public MouseInputForGestureEditor(FrameworkElement sender)
        {
            _context = new MouseInputContext(sender, null, null, null, null, null, null, null);

            _sender = sender;

            this.Normal = new MouseInputNormalForGestureEdit(_context);
            this.Normal.StateChanged += (s, e) => SetState(e.State);

            this.Gesture = new MouseInputGesture(_context);
            this.Gesture.StateChanged += (s, e) => SetState(e.State);

            // initialize state
            _mouseInputCollection = new Dictionary<MouseInputState, MouseInputBase>
            {
                { MouseInputState.Normal, this.Normal },
                { MouseInputState.Gesture, this.Gesture }
            };
            SetState(MouseInputState.Normal);

            // initialize event
            _sender.PreviewMouseDown += OnMouseButtonDown;
            _sender.PreviewMouseUp += OnMouseButtonUp;
            _sender.PreviewMouseWheel += OnMouseWheel;
            _sender.PreviewMouseMove += OnMouseMove;
        }

        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="state"></param>
        public void SetState(MouseInputState state)
        {
            _current?.OnClosed(_sender);

            // 定義されていない場合
            if (!_mouseInputCollection.ContainsKey(state))
            {
                state = MouseInputState.Normal;
            }

            _state = state;
            _current = _mouseInputCollection[_state];
            _current.OnOpened(_sender, null);

            Debug.WriteLine($"{_state} start...");
        }

        /// <summary>
        /// ボタン押したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != _sender) return;
            _current?.OnMouseButtonDown(_sender, e);
        }

        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender != _sender) return;
            _current?.OnMouseButtonUp(_sender, e);
        }

        /// <summary>
        /// ホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender != _sender) return;
            _current?.OnMouseWheel(_sender, e);
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender != _sender) return;
            _current?.OnMouseMove(_sender, e);
        }
    }
}
