using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// マウスジェスチャーシーケンスとコマンドの対応テーブル
    /// </summary>
    public class MouseGestureCommandCollection
    {
        static MouseGestureCommandCollection() => Current = new MouseGestureCommandCollection();
        public static MouseGestureCommandCollection Current { get; }

        /// <summary>
        /// シーケンスとコマンドの対応辞書
        /// </summary>
        private readonly Dictionary<MouseSequence, string> _commands;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseGestureCommandCollection()
        {
            _commands = new Dictionary<MouseSequence, string>();
        }

        /// <summary>
        /// 辞書クリア
        /// </summary>
        public void Clear()
        {
            _commands.Clear();
        }

        /// <summary>
        /// コマンド追加
        /// </summary>
        /// <param name="gestureText"></param>
        /// <param name="command"></param>
        public void Add(MouseSequence gestureText, string command)
        {
            _commands[gestureText] = command;
        }

        /// <summary>
        /// ジェスチャーシーケンスからコマンドを取得
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public string GetCommand(MouseSequence gesture)
        {
            if (gesture == null || gesture.IsEmpty) return "";

            if (_commands.ContainsKey(gesture))
            {
                return _commands[gesture];
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// ジェスチャーシーケンスからコマンドを実行
        /// </summary>
        /// <param name="gesture"></param>
        public void Execute(MouseSequence gesture)
        {
            if (_commands.ContainsKey(gesture))
            {
                var routedCommand = RoutedCommandTable.Current.Commands[_commands[gesture]];
                if (routedCommand != null && routedCommand.CanExecute(null, null))
                {
                    routedCommand.Execute(null, null);
                }
            }
        }

        /// <summary>
        /// マウスジェスチャー通知
        /// </summary>
        public void ShowProgressed(MouseSequence sequence)
        {
            var gesture = sequence.GetDisplayString();

            var commandName = GetCommand(sequence);
            var commandText = RoutedCommandTable.Current.GetFixedRoutedCommand(commandName, true)?.Text;

            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandText)) return;

            InfoMessage.Current.SetMessage(
                InfoMessageType.Gesture,
                ((commandText != null) ? commandText + "\n" : "") + gesture,
                gesture + ((commandText != null) ? " " + commandText : ""));
        }

    }
}
