using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    // Mouse gesture direction
    [TypeConverter(typeof(MouseDirectionConverter))]
    public enum MouseDirection
    {
        None,
        Up,
        Right,
        Down,
        Left,
        Click,
    }

    public static class MouseGestureDirectionExtensions
    {
        private static readonly Dictionary<MouseDirection, string> _map = new()
        {
            [MouseDirection.None] = "",
            [MouseDirection.Up] = "↑",
            [MouseDirection.Right] = "→",
            [MouseDirection.Down] = "↓",
            [MouseDirection.Left] = "←",
            [MouseDirection.Click] = "Click"
        };

        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this MouseDirection key, string value)
        {
            _map[key] = value;
        }

        public static string GetDisplayString(this MouseDirection key)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(key, out var s) ? s : key.ToString());
        }
    }
}
