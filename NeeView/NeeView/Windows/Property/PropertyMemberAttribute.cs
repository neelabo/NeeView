using NeeLaboratory.Collection;
using NeeView.Properties;
using NeeView.Windows.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace NeeView.Windows.Property
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMemberAttribute : Attribute
    {
        public string? Name;
        public string? Title;
        public string? Tips;
        public bool IsVisible = true;
        public string? EmptyMessage;
        public bool HasDecimalPoint;
        public bool IsRegex;
        public Type? NoteConverter;
        public Type? Reset;

        public PropertyMemberAttribute() { }

        public IProeprtyResetable? GetPropertyReset()
        {
            if (Reset is null) return null;

            var resetable = Activator.CreateInstance(Reset) as IProeprtyResetable;
            Debug.Assert(resetable is not null);
            return resetable;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyRangeAttribute : PropertyMemberAttribute
    {
        public double Minimum;
        public double Maximum;
        public double TickFrequency;
        public bool IsEditable;
        public string? Format;
        public string? RangeProperty;

        public PropertyRangeAttribute(double min, double max)
        {
            Minimum = min;
            Maximum = max;
        }
    }

    /// <summary>
    /// double range: percent format
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPercentAttribute : PropertyRangeAttribute
    {
        public PropertyPercentAttribute() : base(0.0, 1.0)
        {
        }

        public PropertyPercentAttribute(double min, double max) : base(min, max)
        {
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPercentFontSizeAttribute : PropertyPercentAttribute
    {
        public FontType FontType;

        public PropertyPercentFontSizeAttribute(FontType fontType) : base(0.0, 1.0)
        {
            FontType = fontType;
        }

        public PropertyPercentFontSizeAttribute(FontType fontType, double min, double max) : base(min, max)
        {
            FontType = fontType;
        }
    }



    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyPathAttribute : PropertyMemberAttribute
    {
        public FileDialogType FileDialogType;
        public string? Filter;
        public string? Note;
        public string? DefaultFileName;

        public PropertyPathAttribute() : base()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyIntegersAttribute : PropertyMemberAttribute
    {
        public int[]? Integers;
        public Type? MapGenerator;

        public PropertyIntegersAttribute() : base()
        {
        }

        public IKeyValueListGenerator<int>? GetMapGenerator()
        {
            if (MapGenerator is null) return null;
            return Activator.CreateInstance(MapGenerator) as IKeyValueListGenerator<int> ?? throw new InvalidOperationException($"Invalid type: {MapGenerator.FullName}");
        }
    }

    public interface IKeyValueListGenerator<T>
    {
        KeyValuePairList<T, string> CreateMap();
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyStringsAttribute : PropertyMemberAttribute
    {
        public string[]? Strings;

        public PropertyStringsAttribute() : base()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyColorAttribute : PropertyMemberAttribute
    {
        public Color? DefaultColor;
        public Type? DefaultColorFactory;

        public PropertyColorAttribute()
        { 
        }

        public Color? GetDefaultColor()
        {
            if (DefaultColor.HasValue)
            {
                return DefaultColor;
            }
        
            if (DefaultColorFactory is not null)
            {
                return (Activator.CreateInstance(DefaultColorFactory) as IColorFactory)?.CreateColor();
            }

            return null;
        }
    }

    public interface IColorFactory
    {
        public Color CreateColor();
    }


    public static class PropertyMemberAttributeExtensions
    {
        private static string GetResourceKey(PropertyInfo property, string? postfix = null)
        {
            return $"{property.DeclaringType?.Name}.{property.Name}{postfix}";
        }

        public static string GetPropertyName(PropertyInfo property, PropertyMemberAttribute? attribute)
        {
            if (attribute is null)
            {
                return TextResources.GetLiteral(property.Name);
            }

            var resourceKey = attribute.Name ?? GetResourceKey(property);
            var resourceValue = TextResources.GetString(resourceKey, true);
            return resourceValue;
        }

        public static string GetPropertyName(PropertyInfo property)
        {
            return GetPropertyName(property, property.GetCustomAttribute<PropertyMemberAttribute>());
        }

        public static string? GetPropertyTips(PropertyInfo property, PropertyMemberAttribute? attribute)
        {
            if (attribute is null)
            {
                return null;
            }

            var resourceKey = attribute.Tips ?? GetResourceKey(property, ".Remarks");
            var resourceValue = TextResources.GetStringRaw(resourceKey);

            return resourceValue;
        }

        public static string? GetPropertyTips(PropertyInfo property)
        {
            return GetPropertyTips(property, property.GetCustomAttribute<PropertyMemberAttribute>());
        }

        public static string? GetPropertyTitle(PropertyInfo property, PropertyMemberAttribute? attribute)
        {
            if (attribute is null)
            {
                return null;
            }

            var resourceKey = attribute.Title ?? GetResourceKey(property, ".Title");
            var resourceValue = TextResources.GetStringRaw(resourceKey);

            return resourceValue;
        }

        public static string? GetPropertyTitle(PropertyInfo property)
        {
            return GetPropertyTitle(property, property.GetCustomAttribute<PropertyMemberAttribute>());
        }
    }
}
