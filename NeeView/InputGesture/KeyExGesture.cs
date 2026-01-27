using System;
using System.ComponentModel;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 拡張キージェスチャ
    /// 単キー対応
    /// </summary>
    [TypeConverter(typeof(KeyExGestureConverter))]
    public class KeyExGesture : InputGesture
    {
        #region Filter

        // 入力許可フラグ
        // NOTE: キーボード入力を一律拒否する挙動のためのフラグだがあまりよろしくない実装
        private static KeyExGestureFilter _filter = KeyExGestureFilter.None;

        public static void AddFilter(KeyExGestureFilter filter)
        {
            if (_filter < filter)
            {
                _filter = filter;
            }
        }

        public static void ResetFilter()
        {
            _filter = KeyExGestureFilter.None;
        }

        #endregion Filter


        // メインキー
        public Key Key { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }


        // コンストラクタ
        public KeyExGesture(Key key) : this(key, ModifierKeys.None)
        {
        }

        public KeyExGesture(Key key, ModifierKeys modifierKeys)
        {
            if (!IsDefinedKey(key)) throw new NotSupportedException();
            Key = key;
            ModifierKeys = modifierKeys;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is not KeyEventArgs keyEventArgs) return false;

            // 入力許可？
            switch (_filter)
            {
                case KeyExGestureFilter.None:
                    break;
                case KeyExGestureFilter.TextKey:
                    if (IsTextKey(keyEventArgs.Key, Keyboard.Modifiers)) return false;
                    break;
                case KeyExGestureFilter.All:
                    if (!IsAllowKey(keyEventArgs.Key, Keyboard.Modifiers)) return false;
                    break;
            }

            // ALTが押されたときはシステムキーを通常キーとする
            Key key = keyEventArgs.Key;
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                key = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
            }

            return this.Key == key && this.ModifierKeys == Keyboard.Modifiers;
        }

        // Esc, Alt+F4 は常に受け入れる
        private static bool IsAllowKey(Key key, ModifierKeys modifiers)
        {
            return key == Key.Escape || (key == Key.F4 && modifiers == ModifierKeys.Alt);
        }

        // 文字キー判定
        private static bool IsTextKey(Key key, ModifierKeys modifiers)
        {
            if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) != 0) return false;
            if (key >= Key.Cancel && key <= Key.Help && key != Key.Space) return false;
            if (key >= Key.F1 && key <= Key.LaunchApplication2) return false;
            if (key >= Key.ImeProcessed) return false;
            return true;
        }


        private static bool IsDefinedKey(Key key)
        {
            return Key.None <= key && key <= Key.OemClear;
        }

        public string GetDisplayString()
        {
            return new KeyGestureSource(Key, ModifierKeys).GetDisplayString();
        }
    }


    public enum KeyExGestureFilter
    {
        // 全て有効
        None,

        // 文字になるキーのみ無効
        TextKey,

        // 全て無効 
        All,
    }

}
