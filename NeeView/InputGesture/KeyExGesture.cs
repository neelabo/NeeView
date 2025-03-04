using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // 入力許可フラグ
        // NOTE: キーボード入力を一律拒否する挙動のためのフラグだがあまりよろしくない実装
        public static bool AllowSingleKey { get; set; }


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

            // 入力許可？ (Escキーは常に受け入れる)
            if (!AllowSingleKey && keyEventArgs.Key != Key.Escape && !IsNormal(keyEventArgs.Key, Keyboard.Modifiers)) return false;

            // ALTが押されたときはシステムキーを通常キーとする
            Key key = keyEventArgs.Key;
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                key = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
            }

            return this.Key == key && this.ModifierKeys == Keyboard.Modifiers;
        }

        // 標準キー判定
        internal static bool IsNormal(Key key, ModifierKeys modifiers)
        {
            if (!((key >= Key.F1 && key <= Key.F24) || (key >= Key.NumPad0 && key <= Key.Divide)))
            {
                if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) != 0)
                {
                    switch (key)
                    {
                        case Key.LeftCtrl:
                        case Key.RightCtrl:
                        case Key.LeftAlt:
                        case Key.RightAlt:
                        case Key.LWin:
                        case Key.RWin:
                            return false;

                        default:
                            return true;
                    }
                }
                else if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.A && key <= Key.Z))
                {
                    return false;
                }
            }
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
}
