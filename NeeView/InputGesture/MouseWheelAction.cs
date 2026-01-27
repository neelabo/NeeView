using System.Collections.Generic;

namespace NeeView
{
    // ホイールアクション
    public enum MouseWheelAction
    {
        None,
        WheelUp,
        WheelDown,
    }

    public static class MouseWheelActionExtensions
    {
        private static readonly Dictionary<MouseWheelAction, string> _map = new();
        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this MouseWheelAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseWheelAction action)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(action, out var s) ? s : action.ToString());
        }
    }
}
