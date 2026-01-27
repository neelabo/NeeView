using System;
using System.ComponentModel;
using System.Windows.Input;

namespace NeeView
{
    // 修飾マウスボタン
    [Flags]
    public enum ModifierMouseButtons
    {
        None = 0,
        LeftButton = (1 << 0),
        MiddleButton = (1 << 1),
        RightButton = (1 << 2),
        XButton1 = (1 << 3),
        XButton2 = (1 << 4),
    }


    public static class ModifierMouseButtonsExtensions
    {
        private const char _modifierDelimiter = '+';

        public static string GetDisplayString(this ModifierMouseButtons modifiers)
        {
            if (!ModifierMouseButtonsConverter.IsDefinedModifierMouseButtons(modifiers))
            {
                throw new InvalidEnumArgumentException(nameof(modifiers), (int)modifiers, typeof(ModifierMouseButtons));
            }

            string strModifiers = "";

            if ((modifiers & ModifierMouseButtons.LeftButton) == ModifierMouseButtons.LeftButton)
            {
                strModifiers += MouseButton.Left.GetDisplayString();
            }

            if ((modifiers & ModifierMouseButtons.MiddleButton) == ModifierMouseButtons.MiddleButton)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MouseButton.Middle.GetDisplayString();
            }

            if ((modifiers & ModifierMouseButtons.RightButton) == ModifierMouseButtons.RightButton)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MouseButton.Right.GetDisplayString();
            }

            if ((modifiers & ModifierMouseButtons.XButton1) == ModifierMouseButtons.XButton1)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MouseButton.XButton1.GetDisplayString();
            }

            if ((modifiers & ModifierMouseButtons.XButton2) == ModifierMouseButtons.XButton2)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MouseButton.XButton2.GetDisplayString();
            }

            return strModifiers;
        }
    }
}
