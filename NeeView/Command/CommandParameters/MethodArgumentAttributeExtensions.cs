using NeeView.Properties;
using System.Reflection;

namespace NeeView
{
    public static class MethodArgumentAttributeExtensions
    {
        private static string GetResourceKey(MethodInfo method, string? postfix = null)
        {
            return $"{method.DeclaringType?.Name}.{method.Name}{postfix}";
        }

        public static string? GetMethodNote(MethodInfo property, MethodArgumentAttribute? attribute)
        {
            if (attribute is null)
            {
                return null;
            }

            var resourceKey = attribute.Note ?? GetResourceKey(property, ".Remarks");
            var resourceValue = TextResources.GetStringRaw(resourceKey);

            return resourceValue;
        }

        public static string? GetMethodNote(MethodInfo property)
        {
            return GetMethodNote(property, property.GetCustomAttribute<MethodArgumentAttribute>());
        }
    }
}
