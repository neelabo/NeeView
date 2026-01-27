using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class TouchAreaConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (context != null && context.Instance != null)
                {
                    return (TouchAreaConverter.IsDefinedTouchArea((TouchArea)context.Instance));
                }
            }
            return false;
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object source)
        {
            if (source != null && source is string s)
            {
                string mouseActionToken = s.Trim();
                mouseActionToken = mouseActionToken.ToUpper(CultureInfo.InvariantCulture);
                if (mouseActionToken == String.Empty)
                {
                    return TouchArea.None;
                }

                switch (mouseActionToken)
                {
                    case "TOUCHL1": return TouchArea.TouchL1;
                    case "TOUCHL2": return TouchArea.TouchL2;
                    case "TOUCHR1": return TouchArea.TouchR1;
                    case "TOUCHR2": return TouchArea.TouchR2;
                    case "TOUCHCENTER": return TouchArea.TouchCenter;
                }
            }
            throw GetConvertFromException(source);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            if (destinationType == typeof(string) && value != null)
            {
                TouchArea key = (TouchArea)value;
                if (TouchAreaConverter.IsDefinedTouchArea(key))
                {
                    switch (key)
                    {
                        case TouchArea.None: return string.Empty;
                        case TouchArea.TouchL1: return "TouchL1";
                        case TouchArea.TouchL2: return "TouchL2";
                        case TouchArea.TouchR1: return "TouchR1";
                        case TouchArea.TouchR2: return "TouchR2";
                        case TouchArea.TouchCenter: return "TouchCenter";
                    }
                }
                throw new InvalidEnumArgumentException(nameof(value), (int)key, typeof(TouchArea));
            }
            throw GetConvertToException(value, destinationType);
        }


        internal static bool IsDefinedTouchArea(TouchArea area)
        {
            return (area >= TouchArea.None && area <= TouchArea.TouchCenter);
        }
    }

}
