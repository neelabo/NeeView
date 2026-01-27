using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    [TypeConverter(typeof(MouseActionConverter))]
    public enum MouseAction
    {
        None,
        LeftClick,
        RightClick,
        MiddleClick,
        WheelClick,
        LeftDoubleClick,
        RightDoubleClick,
        MiddleDoubleClick,
        XButton1Click,
        XButton1DoubleClick,
        XButton2Click,
        XButton2DoubleClick,
        WheelUp,
        WheelDown,
        WheelLeft,
        WheelRight,
    }


    public static class MouseActionExtensions
    {
        private static readonly Dictionary<MouseAction, string> _map = new();
        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static bool IsDefinedKey(this MouseAction key)
        {
            return (int)key >= (int)MouseAction.None && (int)key <= (int)MouseAction.XButton2DoubleClick;
        }

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this MouseAction key, string value)
        {
            _map[key] = value;
        }

        public static string GetDisplayString(this MouseAction key)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(key, out var s) ? s : key.ToString());
        }

        public static MouseExAction ConvertToMouseExAction(this MouseAction key)
        {
            return key switch
            {
                MouseAction.None => MouseExAction.None,
                MouseAction.LeftClick => MouseExAction.LeftClick,
                MouseAction.RightClick => MouseExAction.RightClick,
                MouseAction.MiddleClick => MouseExAction.MiddleClick,
                MouseAction.WheelClick => MouseExAction.WheelClick,
                MouseAction.LeftDoubleClick => MouseExAction.LeftDoubleClick,
                MouseAction.RightDoubleClick => MouseExAction.RightDoubleClick,
                MouseAction.MiddleDoubleClick => MouseExAction.MiddleDoubleClick,
                MouseAction.XButton1Click => MouseExAction.XButton1Click,
                MouseAction.XButton1DoubleClick => MouseExAction.XButton1DoubleClick,
                MouseAction.XButton2Click => MouseExAction.XButton2Click,
                MouseAction.XButton2DoubleClick => MouseExAction.XButton2DoubleClick,
                _ => throw new InvalidCastException($"Cannot convert {nameof(MouseAction)}.{key} to {nameof(MouseExAction)}")
            };
        }

        public static MouseAction ConvertFrom(System.Windows.Input.MouseAction action)
        {
            return action switch
            {
                System.Windows.Input.MouseAction.None => MouseAction.None,
                System.Windows.Input.MouseAction.LeftClick => MouseAction.LeftClick,
                System.Windows.Input.MouseAction.RightClick => MouseAction.RightClick,
                System.Windows.Input.MouseAction.MiddleClick => MouseAction.MiddleClick,
                System.Windows.Input.MouseAction.WheelClick => MouseAction.WheelClick,
                System.Windows.Input.MouseAction.LeftDoubleClick => MouseAction.LeftDoubleClick,
                System.Windows.Input.MouseAction.RightDoubleClick => MouseAction.RightDoubleClick,
                System.Windows.Input.MouseAction.MiddleDoubleClick => MouseAction.MiddleDoubleClick,
                _ => throw new InvalidCastException($"Cannot convert {nameof(System.Windows.Input.MouseAction)}.{action} to {nameof(MouseAction)}")
            };
        }

        public static MouseAction ConvertFrom(MouseExAction action)
        {
            return action switch
            {
                MouseExAction.None => MouseAction.None,
                MouseExAction.LeftClick => MouseAction.LeftClick,
                MouseExAction.RightClick => MouseAction.RightClick,
                MouseExAction.MiddleClick => MouseAction.MiddleClick,
                MouseExAction.WheelClick => MouseAction.WheelClick,
                MouseExAction.LeftDoubleClick => MouseAction.LeftDoubleClick,
                MouseExAction.RightDoubleClick => MouseAction.RightDoubleClick,
                MouseExAction.MiddleDoubleClick => MouseAction.MiddleDoubleClick,
                MouseExAction.XButton1Click => MouseAction.XButton1Click,
                MouseExAction.XButton1DoubleClick => MouseAction.XButton1DoubleClick,
                MouseExAction.XButton2Click => MouseAction.XButton2Click,
                MouseExAction.XButton2DoubleClick => MouseAction.XButton2DoubleClick,
                _ => throw new InvalidCastException($"Cannot convert {nameof(MouseExAction)}.{action} to {nameof(MouseAction)}")
            };
        }

        public static MouseAction ConvertFrom(MouseWheelAction action)
        {
            return action switch
            {
                MouseWheelAction.None => MouseAction.None,
                MouseWheelAction.WheelUp => MouseAction.WheelUp,
                MouseWheelAction.WheelDown => MouseAction.WheelDown,
                _ => throw new InvalidCastException($"Cannot convert {nameof(MouseWheelAction)}.{action} to {nameof(MouseAction)}")
            };
        }

        public static MouseAction ConvertFrom(MouseHorizontalWheelAction action)
        {
            return action switch
            {
                MouseHorizontalWheelAction.None => MouseAction.None,
                MouseHorizontalWheelAction.WheelLeft => MouseAction.WheelLeft,
                MouseHorizontalWheelAction.WheelRight => MouseAction.WheelRight,
                _ => throw new InvalidCastException($"Cannot convert {nameof(MouseHorizontalWheelAction)}.{action} to {nameof(MouseAction)}")
            };
        }
    }
}
