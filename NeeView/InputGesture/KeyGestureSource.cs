using System.ComponentModel;
using System.Windows.Input;

namespace NeeView
{
    [TypeConverter(typeof(KeyGestureSourceConverter))]
    public record class KeyGestureSource : InputGestureSource
    {
        private const char _modifiersDelimiter = '+';

        public KeyGestureSource(Key key) : this(key, ModifierKeys.None)
        {
        }

        public KeyGestureSource(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public Key Key { get; }

        public ModifierKeys Modifiers { get; }

        public override InputGesture GetInputGesture()
        {
            return new KeyExGesture(Key, Modifiers);
        }

        public override string GetDisplayString()
        {
            if (Key == Key.None) return "";

            string strBinding = "";
            string strKey = Key.GetDisplayString();
            if (strKey != "")
            {
                strBinding += Modifiers.GetDisplayString();
                if (strBinding != "")
                {
                    strBinding += _modifiersDelimiter;
                }
                strBinding += strKey;
            }
            return strBinding;
        }
    }

}
