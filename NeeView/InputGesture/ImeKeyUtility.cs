using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    public static class ImeKeyUtility
    {
        /// <summary>
        /// 管理するIMEキー
        /// </summary>
        private static readonly List<Key> _imeKeys = new() { Key.ImeConvert, Key.ImeNonConvert };

        /// <summary>
        /// 入力定義にIMEキーが入っている？
        /// </summary>
        public static bool HasImeKey(this InputGesture gesture)
        {
            if (gesture is KeyGesture keyGesture)
            {
                return _imeKeys.Contains(keyGesture.Key);
            }
            else if (gesture is KeyExGesture keyExGesture)
            {
                return _imeKeys.Contains(keyExGesture.Key);
            }

            return false;
        }

        /// <summary>
        /// IMEキー？
        /// </summary>
        public static bool IsImeKey(this Key key)
        {
            return _imeKeys.Contains(key);
        }
    }
}
