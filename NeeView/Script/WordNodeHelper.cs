using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public static class WordNodeHelper
    {
        public static WordNode CreateClassWordNode(string name, Type type)
        {
            var node = new WordNode(name);
            node.Children = new List<WordNode>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties.Where(e => !IsObsolete(e)))
            {
                var attribute = property.GetCustomAttribute<WordNodeMemberAttribute>();
                if (attribute != null && attribute.IsAutoCollect)
                {
                    if (IsSingleValueType(property.PropertyType))
                    {
                        var propertyMemberAttribute = property.PropertyType.GetCustomAttribute<WordNodeMemberAttribute>();
                        if (propertyMemberAttribute != null && propertyMemberAttribute.IsAutoCollect)
                        {
                            node.Children.Add(CreateClassWordNode(property.Name, property.PropertyType));
                            continue;
                        }
                    }

                    node.Children.Add(new WordNode(property.Name));
                }
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods.Where(e => !IsObsolete(e)))
            {
                var attribute = method.GetCustomAttribute<WordNodeMemberAttribute>();
                if (attribute != null && attribute.IsAutoCollect)
                {
                    node.Children.Add(new WordNode(method.Name));
                }
            }

            return node;
        }

        private static bool IsObsolete(MemberInfo info)
        {
            return info.GetCustomAttribute<ObsoleteAttribute>() != null;
        }

        private static bool IsSingleValueType(Type type)
        {
            if (type.IsArray)
            {
                return false;
            }
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                return false;
            }
            return true;
        }
    }
}
