using System.ComponentModel;
using System.Windows.Input;

namespace NeeView
{
    [TypeConverter(typeof(MouseGestureSourceConverter))]
    public record class MouseGestureSource : InputGestureSource
    {
        private const char _modifiersDelimiter = '+';

        public MouseGestureSource(MouseAction action) : this(action, ModifierKeys.None, ModifierMouseButtons.None)
        {
        }

        public MouseGestureSource(MouseAction action, ModifierKeys modifierKeys) : this(action, modifierKeys, ModifierMouseButtons.None)
        {
        }

        public MouseGestureSource(MouseAction action, ModifierKeys modifiers, ModifierMouseButtons modifierButtons)
        {
            Action = action;
            Modifiers = modifiers;
            ModifierButtons = modifierButtons;
        }

        public MouseAction Action { get; }
        public ModifierKeys Modifiers { get; }
        public ModifierMouseButtons ModifierButtons { get; }

        public override InputGesture GetInputGesture()
        {
            switch (Action)
            {
                case MouseAction.WheelUp:
                    return new MouseWheelGesture(MouseWheelAction.WheelUp, Modifiers, ModifierButtons);
                case MouseAction.WheelDown:
                    return new MouseWheelGesture(MouseWheelAction.WheelDown, Modifiers, ModifierButtons);
                case MouseAction.WheelLeft:
                    return new MouseHorizontalWheelGesture(MouseHorizontalWheelAction.WheelLeft, Modifiers, ModifierButtons);
                case MouseAction.WheelRight:
                    return new MouseHorizontalWheelGesture(MouseHorizontalWheelAction.WheelRight, Modifiers, ModifierButtons);
                default:
                    return new MouseExGesture(Action.ConvertToMouseExAction(), Modifiers, ModifierButtons);
            }
        }

        public override string GetDisplayString()
        {
            if (Action == MouseAction.None) return "";

            string strBinding = "";
            string? strKey = Action.GetDisplayString();
            if (strKey != string.Empty)
            {
                strBinding += Modifiers.GetDisplayString();
                if (strBinding != string.Empty)
                {
                    strBinding += _modifiersDelimiter;
                }

                var buttons = ModifierButtons.GetDisplayString();
                if (buttons != string.Empty)
                {
                    strBinding += buttons;
                    strBinding += _modifiersDelimiter;
                }

                strBinding += strKey;
            }
            return strBinding;
        }
    }

}
