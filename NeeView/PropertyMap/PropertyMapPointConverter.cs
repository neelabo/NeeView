﻿using System;
using System.Windows;

namespace NeeView
{
    public class PropertyMapPointConverter : PropertyMapConverter<Point>
    {
        public override string GetTypeName(Type typeToConvert)
        {
            return "\"x,y\"";
        }

        public override object? Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue()?.ToString();
        }

        public override void Write(PropertyMapSource source, object? value, PropertyMapOptions options)
        {
            if (value is null) throw new NotSupportedException("Cannot convert from null");
            source.SetValue((Point?)new PointConverter().ConvertFrom(value));
        }
    }


}
