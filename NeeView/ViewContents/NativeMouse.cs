using NeeView.Interop;
using System.Collections.Generic;

namespace NeeView
{
    public static class NativeMouse
    {
        public enum VirtualKeyCode
        {
            LeftButton = NativeMethods.VK_LBUTTON,
            RightButton = NativeMethods.VK_RBUTTON,
            MiddleButton = NativeMethods.VK_MBUTTON,
            XButton1 = NativeMethods.VK_XBUTTON1,
            XButton2 = NativeMethods.VK_XBUTTON2,
        }

        public static Dictionary<MouseButtonBits, VirtualKeyCode> _keyMap = new()
        {
            [MouseButtonBits.LeftButton] = VirtualKeyCode.LeftButton,
            [MouseButtonBits.RightButton] = VirtualKeyCode.RightButton,
            [MouseButtonBits.MiddleButton] = VirtualKeyCode.MiddleButton,
            [MouseButtonBits.XButton1] = VirtualKeyCode.XButton1,
            [MouseButtonBits.XButton2] = VirtualKeyCode.XButton2,
        };

        public static bool IsPressed(VirtualKeyCode code) => (NativeMethods.GetKeyState((int)code) & 0xFF00) == 0xFF00;

        public static bool AnyPressed(MouseButtonBits bits)
        {
            foreach (var pair in _keyMap)
            {
                if ((bits & pair.Key) != 0)
                {
                    if (IsPressed(pair.Value)) return true;
                }
            }
            return false;
        }
    }
}
