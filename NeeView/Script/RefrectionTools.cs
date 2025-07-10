using NeeView.Properties;
using System;
using System.Globalization;
using System.Reflection;

namespace NeeView
{
    public static class RefrectionTools
    {
        public static ObsoleteAttribute? GetPropertyObsoleteAttribute(object source, string name)
        {
            var type = source.GetType();
            return type.GetProperty(name)?.GetCustomAttribute<ObsoleteAttribute>();
        }

        public static ObsoleteAttribute? GetMethodObsoleteAttribute(object source, string name)
        {
            var type = source.GetType();
            return type.GetMethod(name)?.GetCustomAttribute<ObsoleteAttribute>();
        }

        public static string? CreatePropertyObsoleteMessage(Type type, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            var propertyInfo = type.GetProperty(name);
            if (propertyInfo is null) throw new ArgumentException($"No such property: {name}");

            var obsolete = propertyInfo.GetCustomAttribute<ObsoleteAttribute>();
            var alternative = propertyInfo.GetCustomAttribute<AlternativeAttribute>();

            return CreateObsoleteMessage(name, obsolete, alternative);
        }

        public static string? CreateMethodObsoleteMessage(Type type, [System.Runtime.CompilerServices.CallerMemberName]  string? name = null)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            var methodInfo = type.GetMethod(name);
            if (methodInfo is null) throw new ArgumentException($"No such method: {name}");

            var obsolete = methodInfo.GetCustomAttribute<ObsoleteAttribute>();
            var alternative = methodInfo.GetCustomAttribute<AlternativeAttribute>();

            return CreateObsoleteMessage(name + "()", obsolete, alternative);
        }


        public static string? CreateObsoleteMessage(string name, ObsoleteAttribute? obsolete, AlternativeAttribute? alternative)
        {
            if (obsolete is null) return null;

            var message = string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ScriptErrorMessage.Obsolete"), name);
            if (alternative?.Alternative != null)
            {
                message += " " + string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ScriptErrorMessage.Alternative"), alternative.Alternative);
            }

            return message;
        }

    }



}
