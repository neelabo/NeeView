using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

// TODO: CommandTable.Current をコンストラクタに渡す

namespace NeeView
{
    /// <summary>
    /// コマンド集 ： RoutedCommand
    /// </summary>
    public partial class RoutedCommandTable : IDisposable
    {
        static RoutedCommandTable() => Current = new RoutedCommandTable();
        public static RoutedCommandTable Current { get; }


        private HashSet<Key> _usedKeyMap = new();
        private bool _isDirty = true;
        private List<EventHandler<KeyEventArgs>> _imeKeyHandlers = new();
        private readonly MouseWheelDelta _mouseVerticalWheelDelta = new();
        private readonly MouseWheelDelta _mouseHorizontalWheelDelta = new();
        private readonly List<TouchInput> _touchInputCollection = new();
        private readonly List<MouseInput> _mouseInputCollection = new();
        private bool _disposedValue;

        private RoutedCommandTable()
        {
            // RoutedCommand作成
            foreach (var command in CommandTable.Current)
            {
                Commands.Add(command.Key, new RoutedUICommand(command.Value.Text, command.Key, typeof(MainWindow)));
            }

            // コマンド変更でショートカット変更
            CommandTable.Current.Changed += CommandTable_Changed;

            UpdateInputGestures();
        }


        /// <summary>
        /// コマンドテーブルが更新されたときのイベント
        /// </summary>
        [Subscribable]
        public event EventHandler? Changed;

        /// <summary>
        /// コマンドが実行されたときのイベント
        /// </summary>
        [Subscribable]
        public event EventHandler<CommandExecutedEventArgs>? CommandExecuted;


        /// <summary>
        /// コマンド辞書
        /// </summary>
        public Dictionary<string, RoutedUICommand> Commands { get; set; } = new Dictionary<string, RoutedUICommand>();



        private void CommandTable_Changed(object? sender, CommandChangedEventArgs e)
        {
            if (_disposedValue) return;

            _isDirty = true;

            if (!e.OnHold)
            {
                UpdateInputGestures();
            }
        }

        public void SetDirty()
        {
            if (_disposedValue) return;

            _isDirty = true;
        }

        public void UpdateRoutedCommand()
        {
            if (_disposedValue) return;

            var oldies = Commands.Keys
                .ToList();

            var news = CommandTable.Current.Keys
                .ToList();

            foreach (var name in oldies.Except(news))
            {
                Commands.Remove(name);
            }

            foreach (var name in news.Except(oldies))
            {
                var command = CommandTable.Current.GetElement(name) ?? throw new InvalidOperationException();
                Commands.Add(name, new RoutedUICommand(command.Text, name, typeof(MainWindow)));
            }
        }




        public void AddTouchInput(TouchInput touchInput)
        {
            if (_disposedValue) return;

            _touchInputCollection.Add(touchInput);
            UpdateTouchInputGestures(touchInput);
        }

        public void AddMouseInput(MouseInput mouseInput)
        {
            if (_disposedValue) return;

            _mouseInputCollection.Add(mouseInput);
            UpdateMouseInputGestures(mouseInput);
        }


        // InputGesture設定
        public void UpdateInputGestures()
        {
            if (_disposedValue) return;

            if (!_isDirty) return;
            _isDirty = false;

            UpdateRoutedCommand();
            ClearRoutedCommandInputGestures();

            UpdateMouseDragGestures();

            foreach (var touchInput in _touchInputCollection)
            {
                UpdateTouchInputGestures(touchInput);
            }

            foreach (var mouseInput in _mouseInputCollection)
            {
                UpdateMouseInputGestures(mouseInput);
            }

            UpdateKeyInputGestures();

            Changed?.Invoke(this, EventArgs.Empty);
        }


        private void ClearRoutedCommandInputGestures()
        {
            foreach (var command in this.Commands)
            {
                command.Value.InputGestures.Clear();
            }
        }

        private void UpdateMouseDragGestures()
        {
            MouseGestureCommandCollection.Current.Clear();

            foreach (var command in this.Commands)
            {
                var mouseGesture = CommandTable.Current.GetElement(command.Key).MouseGesture;
                if (mouseGesture != null)
                {
                    MouseGestureCommandCollection.Current.Add(mouseGesture, command.Key);
                }
            }
        }

        private void UpdateTouchInputGestures(TouchInput touch)
        {
            touch.ClearTouchEventHandler();

            foreach (var command in this.Commands)
            {
                var areas = CommandTable.Current.GetElement(command.Key).TouchGesture.Areas;
                foreach (var area in areas)
                {
                    touch.TouchGestureChanged += (s, x) =>
                    {
                        if (command.Key == "TouchEmulate") return;

                        if (!x.Handled && x.Area == area)
                        {
                            command.Value.Execute(null, (s as IInputElement) ?? MainWindow.Current);
                            x.Handled = true;
                        }
                    };
                }
            }
        }

        private void UpdateMouseInputGestures(MouseInput mouse)
        {
            mouse.ClearMouseEventHandler();

            var mouseNormalHandlers = new List<EventHandler<MouseButtonEventArgs>>();
            var mouseExtraHandlers = new List<EventHandler<MouseButtonEventArgs>>();

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current.GetElement(command.Key).ShortCutKey.Gestures.Select(e => e.GetInputGesture());
                foreach (var gesture in inputGestures)
                {
                    if (gesture is MouseGesture)
                    {
                        mouseNormalHandlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                    }
                    else if (gesture is MouseExGesture)
                    {
                        mouseExtraHandlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                    }
                    else if (gesture is MouseWheelGesture)
                    {
                        mouse.MouseWheelChanged += (s, x) =>
                        {
                            if (!x.Handled && gesture.Matches(this, x))
                            {
                                var wheelOptions = MouseWheelDeltaOption.None;
                                VerticalWheelCommandExecute(s, x, wheelOptions, command.Value);
                            }
                        };
                    }
                    else if (gesture is MouseHorizontalWheelGesture)
                    {
                        mouse.MouseHorizontalWheelChanged += (s, x) =>
                        {
                            if (!x.Handled && gesture.Matches(this, x))
                            {
                                var wheelOptions = Config.Current.Command.IsHorizontalWheelLimitedOnce ? MouseWheelDeltaOption.LimitOnce : MouseWheelDeltaOption.None;
                                HorizontalWheelCommandExecute(s, x, wheelOptions, command.Value);
                            }
                        };
                    }
                }
            }

            // NOTE: 拡張マウス入力から先に処理を行う
            foreach (var lambda in mouseExtraHandlers.Concat(mouseNormalHandlers))
            {
                mouse.MouseButtonChanged += lambda;
            }
        }


        /// <summary>
        /// Initialize KeyInput gestures
        /// </summary>
        private void UpdateKeyInputGestures()
        {
            var imeKeyHandlers = new List<EventHandler<KeyEventArgs>>();

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current.GetElement(command.Key).ShortCutKey.Gestures.OfType<KeyGestureSource>().Select(e => e.GetInputGesture());
                foreach (var gesture in inputGestures)
                {
                    if (gesture.HasImeKey())
                    {
                        imeKeyHandlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                    }
                    command.Value.InputGestures.Add(gesture);
                }
            }

            _imeKeyHandlers = imeKeyHandlers;

            UpdateUsedKeyMap();
        }


        // コマンドで使用されているキーマップ生成
        private void UpdateUsedKeyMap()
        {
            var map = new HashSet<Key>();

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current.GetElement(command.Key).ShortCutKey.Gestures.OfType<KeyGestureSource>().Select(e => e.GetInputGesture());
                foreach (var gesture in inputGestures)
                {
                    switch (gesture)
                    {
                        case KeyGesture keyGesture:
                            map.Add(keyGesture.Key);
                            break;
                        case KeyExGesture keyExGesture:
                            map.Add(keyExGesture.Key);
                            break;
                    }
                }
            }

            _usedKeyMap = map;
        }

        // コマンドで使用されているキー？
        public bool IsUsedKey(Key key)
        {
            if (_disposedValue) return false;

            return _usedKeyMap.Contains(key);
        }

        // IMEキーコマンドを直接実行
        public void ExecuteImeKeyGestureCommand(object? sender, KeyEventArgs args)
        {
            if (_disposedValue) return;

            foreach (var handle in _imeKeyHandlers)
            {
                if (args.Handled) return;
                handle.Invoke(sender, args);
            }
        }

        // コマンドのジェスチャー判定と実行
        private void InputGestureCommandExecute(object? sender, InputEventArgs x, InputGesture gesture, RoutedUICommand command)
        {
            if (_disposedValue) return;

            if (!x.Handled && gesture.Matches(this, x))
            {
                command.Execute(null, (sender as IInputElement) ?? MainWindow.Current);
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(gesture));
                if (x.RoutedEvent != null)
                {
                    x.Handled = true;
                }
            }
        }

        // 垂直ホイールの回転数に応じたコマンド実行
        private void VerticalWheelCommandExecute(object? sender, MouseWheelEventArgs arg, MouseWheelDeltaOption wheelOptions, RoutedUICommand command)
        {
            WheelCommandExecute(sender, arg, _mouseVerticalWheelDelta, wheelOptions, command, Config.Current.Command.IsReversePageMoveWheel);
        }

        // 水平ホイールの回転数に応じたコマンド実行
        private void HorizontalWheelCommandExecute(object? sender, MouseWheelEventArgs arg, MouseWheelDeltaOption wheelOptions, RoutedUICommand command)
        {
            WheelCommandExecute(sender, arg, _mouseHorizontalWheelDelta, wheelOptions, command, Config.Current.Command.IsReversePageMoveHorizontalWheel);
        }

        // ホイールの回転数に応じたコマンド実行
        private void WheelCommandExecute(object? sender, MouseWheelEventArgs arg, MouseWheelDelta wheelDelta, MouseWheelDeltaOption wheelOptions, RoutedUICommand command, bool allowFlip)
        {
            if (_disposedValue) return;

            int turn = Math.Abs(wheelDelta.NotchCount(arg, wheelOptions));
            if (turn > 0)
            {
                // Debug.WriteLine($"WheelCommand: {turn}({arg.Delta})");
                var param = new CommandParameterArgs(null, allowFlip);
                for (int i = 0; i < turn; i++)
                {
                    command.Execute(param, (sender as IInputElement) ?? MainWindow.Current);
                }
            }

            if (arg.RoutedEvent != null)
            {
                arg.Handled = true;
            }
        }

        // コマンド実行 
        // CommandTableを純粋なコマンド定義のみにするため、コマンド実行に伴う処理はここで定義している
        public void Execute(object sender, string name, object parameter)
        {
            if (_disposedValue) return;

            bool allowFlip = (parameter is CommandParameterArgs args)
                ? args.AllowFlip
                : (parameter != MenuCommandTag.Tag);

            var command = CommandTable.Current.GetElement(GetFixedCommandName(name, allowFlip));

            // 通知
            if (command.IsShowMessage)
            {
                string message = command.ExecuteMessage(sender, CommandArgs.Empty);
                if (!string.IsNullOrEmpty(message))
                {
                    InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
                }
            }

            // 実行
            var option = (parameter is MenuCommandTag) ? CommandOption.ByMenu : CommandOption.None;
            command.Execute(sender, new CommandArgs(null, option));
        }

        // スライダー方向によって移動コマンドを入れ替える
        private static string GetFixedCommandName(string name, bool allowFlip)
        {
            if (allowFlip && Config.Current.Command.IsReversePageMove && MainWindowModel.Current.IsLeftToRightSlider())
            {
                CommandTable.Current.TryGetValue(name, out var command);
                if (command != null && command.PairPartner != null)
                {
                    if (command.Parameter is ReversibleCommandParameter reversibleCommandParameter)
                    {
                        return reversibleCommandParameter.IsReverse ? command.PairPartner : name;
                    }
                    else
                    {
                        return command.PairPartner;
                    }
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return name;
            }
        }

        public CommandElement? GetFixedCommandElement(string commandName, bool allowRecursive)
        {
            if (_disposedValue) return null;

            CommandTable.Current.TryGetValue(GetFixedCommandName(commandName, allowRecursive), out CommandElement? command);
            return command;
        }

        public RoutedUICommand? GetFixedRoutedCommand(string commandName, bool allowRecursive)
        {
            if (_disposedValue) return null;

            this.Commands.TryGetValue(GetFixedCommandName(commandName, allowRecursive), out RoutedUICommand? command);
            return command;
        }



        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CommandTable.Current.Changed -= CommandTable_Changed;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
