using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    [TypeConverter(typeof(TouchAreaConverter))]
    public enum TouchArea
    {
        None,
        TouchL1,
        TouchL2,
        TouchR1,
        TouchR2,
        TouchCenter,
    }


    public static class TouchAreaExtensions
    {
        private static readonly Dictionary<TouchArea, string> _map = new();
        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this TouchArea key, string value)
        {
            _map[key] = value;
        }

        public static string GetDisplayString(this TouchArea key)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(key, out var s) ? s : key.ToString());
        }

        public static TouchArea GetTouchArea(double xRate, double yRate)
        {
            return TouchArea.TouchCenter.IsTouched(xRate, yRate)
                ? TouchArea.TouchCenter
                : GetTouchAreaLast(xRate, yRate);
        }

        public static TouchArea GetTouchAreaLast(double xRate, double yRate)
        {
            return xRate < 0.5
                ? yRate < 0.5 ? TouchArea.TouchL1 : TouchArea.TouchL2
                : yRate < 0.5 ? TouchArea.TouchR1 : TouchArea.TouchR2;
        }

        public static bool IsTouched(this TouchArea self, double xRate, double yRate)
        {
            return self switch
            {
                TouchArea.TouchCenter => 0.33 < xRate && xRate < 0.66 && yRate < 0.75,
                TouchArea.TouchL1 => xRate < 0.5 && yRate < 0.5,
                TouchArea.TouchL2 => xRate < 0.5 && !(yRate < 0.5),
                TouchArea.TouchR1 => !(xRate < 0.5) && yRate < 0.5,
                TouchArea.TouchR2 => !(xRate < 0.5) && !(yRate < 0.5),
                _ => false,
            };
        }
    }


}
