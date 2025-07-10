using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class PropertyMapSource : PropertyMapNode
    {
        private readonly string? _prefix;

        public PropertyMapSource(string name, ObsoleteAttribute? obsolete, AlternativeAttribute? alternative, object source, PropertyInfo property, PropertyMapConverter converter, string? prefix)
            : base(name, obsolete, alternative)
        {
            Source = source;
            PropertyInfo = property;
            IsReadOnly = property.GetCustomAttribute(typeof(PropertyMapReadOnlyAttribute)) != null;
            Converter = converter;
            _prefix = prefix;
        }

        public object Source { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public bool IsReadOnly { get; private set; }

        public PropertyMapConverter Converter { get; private set; }


        public object? Read(PropertyMapOptions options)
        {
            return Converter.Read(this, PropertyInfo.PropertyType, options);
        }

        public void Write(object? value, PropertyMapOptions options)
        {
            if (IsReadOnly) return;
            Converter.Write(this, value, options);
        }

        public object? GetValue()
        {
            return PropertyInfo.GetValue(Source);
        }

        public void SetValue(object? value)
        {
            PropertyInfo.SetValue(Source, value);
        }

        public (string type, string description) CreateHelpHtml()
        {
            string typeString;
            if (PropertyInfo.PropertyType.IsEnum)
            {
                typeString = "<dl>" + string.Join("", PropertyInfo.PropertyType.VisibleAliasNameDictionary().Select(e => $"<dt>\"{e.Key}\"</dt><dd>{e.Value}</dd>")) + "</dl>";
            }
            else
            {
                typeString = Converter.GetTypeName(PropertyInfo.PropertyType);
            }

            var readOnly = (PropertyInfo.GetCustomAttribute<PropertyMapReadOnlyAttribute>() != null || !PropertyInfo.CanWrite) ? " (" + TextResources.GetString("Word.ReadOnly") + ")" : "";

            var description = "";
            var attribute = PropertyInfo.GetCustomAttribute<PropertyMemberAttribute>();
            if (attribute is not null)
            {
                var name = "<b>" + _prefix + PropertyMemberAttributeExtensions.GetPropertyName(PropertyInfo, attribute) + readOnly + "</b>";
                var tips = PropertyMemberAttributeExtensions.GetPropertyTips(PropertyInfo, attribute);
                description = name;
                if (!string.IsNullOrEmpty(tips))
                {
                    description += "<p class=\"remarks\">" + PropertyMemberAttributeExtensions.GetPropertyTips(PropertyInfo, attribute) + "</p>";
                }
            }

            return (typeString, description);
        }
    }
}
