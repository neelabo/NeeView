using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// 
    /// </summary>
    public class PropertyDocument
    {
        public PropertyDocument()
        {
            this.Elements = new List<PropertyDrawElement>();
        }

        public PropertyDocument(object source)
        {
            this.Source = source;
            this.Elements = CreatePropertyContentList(source);
        }

        public PropertyDocument(IEnumerable<object> sources)
        {
            this.Elements = sources.Select(e => CreatePropertyContentList(e)).SelectMany(e => e).ToList();
        }


        // name
        //public string Name { get; set; }

        // class source
        public object? Source { get; set; }

        // properties
        public List<PropertyDrawElement> Elements { get; set; }

        // properties (member only)
        public List<PropertyMemberElement> PropertyMembers => Elements.OfType<PropertyMemberElement>().ToList();

        public PropertyDrawElement? this[string key]
        {
            get => GetPropertyMember(key);
        }


        public PropertyMemberElement? GetPropertyMember(string path)
        {
            return Elements.OfType<PropertyMemberElement>().FirstOrDefault(e => e.Path == path);
        }

        /// <summary>
        /// 上書き
        /// </summary>
        /// <param name="source">元となるパラメータ</param>
        public void Set(object source)
        {
            Debug.Assert(Source is null || Source.GetType() == source.GetType());
            foreach (var element in Elements)
            {
                if (element is PropertyMemberElement property)
                {
                    property.SetValue(property.GetValue(source));
                }
            }
        }

        /// <summary>
        /// 全ての設定値を初期化
        /// </summary>
        public void Reset()
        {
            foreach (var item in this.Elements.OfType<PropertyMemberElement>())
            {
                item.ResetValue();
            }
        }


        public void SetVisualType<T>(string visualType)
        {
            foreach (var propertyValue in this.PropertyMembers.Select(e => e.TypeValue).Where(e => e is T))
            {
                propertyValue.VisualType = visualType;
            }
        }


        private static List<PropertyDrawElement> CreatePropertyContentList(object source)
        {
            var type = source.GetType();

            var list = new List<PropertyDrawElement>();

            foreach (PropertyInfo info in type.GetProperties())
            {
                var attribute = GetPropertyMemberAttribute(info);
                if (attribute != null)
                {
                    var title = PropertyMemberAttributeExtensions.GetPropertyTitle(info, attribute);
                    if (title != null)
                    {
                        list.Add(new PropertyTitleElement(title));
                    }

                    var element = new PropertyMemberElement(source, info, attribute, PropertyMemberElementOptions.Default);
                    if (element.IsVisible)
                    {
                        list.Add(element);
                    }
                    else
                    {
                        ////Debug.WriteLine($"PropertyDocument: {element.Name} is Hide.");
                    }
                }
            }
            return list;
        }

        public void AddProperty(object source, string propertyName)
        {
            var element = CreatePropertyMemberElement(source, propertyName);
            Debug.Assert(element != null);
            if (element != null)
            {
                this.Elements.Add(element);
            }
        }

        private static PropertyMemberElement? CreatePropertyMemberElement(object source, string propertyName)
        {
            var type = source.GetType();
            var info = type.GetProperty(propertyName);
            if (info == null) return null;

            var attribute = GetPropertyMemberAttribute(info);
            if (attribute is null) return null;
            return new PropertyMemberElement(source, info, attribute, PropertyMemberElementOptions.Default);
        }

        private static PropertyMemberAttribute? GetPropertyMemberAttribute(MemberInfo info)
        {
            return (PropertyMemberAttribute?)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
        }
    }

    public static class PropertyVisualType
    {
        public const string ToggleSwitch = "ToggleSwitch";
        public const string ComboColorPicker = "ComboColorPicker";
    }

}
