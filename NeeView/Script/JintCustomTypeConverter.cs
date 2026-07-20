using Jint;
using Jint.Runtime.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace NeeView
{
    public class JintCustomTypeConverter : DefaultTypeConverter
    {
        public JintCustomTypeConverter(Engine engine) : base(engine)
        {
        }

        public override object? Convert(object? value, Type type, IFormatProvider formatProvider)
        {
            if (type == typeof(Color) && value is string s)
            {
                try
                {
                    return ColorConverter.ConvertFromString(s);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Could not convert '{s}' to type Color.", ex);
                }
            }

            return base.Convert(value, type, formatProvider);
        }

        public override bool TryConvert(object? value, Type type, IFormatProvider formatProvider, [NotNullWhen(true)] out object? converted)
        {
            if (type == typeof(Color) && value is string s)
            {
                try
                {
                    converted = ColorConverter.ConvertFromString(s);
                    return true;
                }
                catch
                {
                    converted = null;
                    return false;
                }
            }

            return base.TryConvert(value, type, formatProvider, out converted);
        }
    }
}
