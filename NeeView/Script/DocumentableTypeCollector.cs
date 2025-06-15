using NeeLaboratory.Collection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public static class DocumentableTypeCollector
    {
        public static List<Type> Collect(Type type)
        {
            var collection = new List<Type>();
            Collect(collection, type);
            return collection.Where(e => IsDocumentableType(e)).ToList();
        }

        private static void Collect(List<Type> collection, Type type)
        {
            var types = CollectAnchorTypes(type);
            foreach (var t in types)
            {
                if (!collection.Contains(t))
                {
                    collection.Add(t);
                    Collect(collection, t);
                }
            }
        }

        public static List<Type> CollectAnchorTypes(Type type)
        {
            var properties = type.GetProperties().Where(e => IsDocumentable(e)).OrderBy(e => e.Name);
            var methods = type.GetMethods().Where(e => IsDocumentable(e)).OrderBy(e => e.Name);

            var types = new List<Type>();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<DocumentableAttribute>();
                types.AddIfNotNull(GetAnchorType(attribute?.DocumentType ?? property.PropertyType));
                var returnTypeAttribute = property.GetCustomAttribute<ReturnTypeAttribute>();
                if (returnTypeAttribute is not null)
                {
                    foreach (var returnType in returnTypeAttribute.ReturnTypes)
                    {
                        types.AddIfNotNull(GetAnchorType(returnType));
                    }
                }
            }

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<DocumentableAttribute>();
                types.AddIfNotNull(GetAnchorType(attribute?.DocumentType ?? method.ReturnType));
                var returnTypeAttribute = method.GetCustomAttribute<ReturnTypeAttribute>();
                if (returnTypeAttribute is not null)
                {
                    foreach (var returnType in returnTypeAttribute.ReturnTypes)
                    {
                        types.AddIfNotNull(GetAnchorType(returnType));
                    }
                }
            }

            {
                var attribute = type.GetCustomAttribute<DocumentableDerivedClassAttribute>();
                if (attribute is not null && attribute.DerivedClass.Any())
                {
                    types.AddRange(attribute.DerivedClass);
                }
            }

            return types.Distinct().ToList();
        }

        private static bool IsDocumentable(MemberInfo info)
        {
            return info.GetCustomAttribute<DocumentableAttribute>() != null && info.GetCustomAttribute<ObsoleteAttribute>() == null;
        }

        private static Type? GetAnchorType(Type type)
        {
            if (type == typeof(void))
            {
                return null;
            }

            if (type == typeof(string))
            {
                return type;
            }

            // array
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType is null) throw new InvalidOperationException();
                type = elementType;
            }

            // collection
            var propertyIndexer = type.GetProperties().Where(p => p.GetIndexParameters().Length != 0).FirstOrDefault();
            if (propertyIndexer != null)
            {
                type = propertyIndexer.PropertyType;
            }

            return GetDocumentType(type);
        }

        /// <summary>
        /// ドキュメントにする型であるかを判定
        /// </summary>
        public static bool IsDocumentableType(Type type)
        {
            if (type.IsEnum) return true;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Double:
                case TypeCode.String:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    return false;
            }

            if (type == typeof(object))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 属性による名前指定を反映
        /// </summary>
        private static Type GetDocumentType(Type type)
        {
            var attribute = type.GetCustomAttribute<DocumentableAttribute>();
            if (attribute != null && attribute.DocumentType != null)
            {
                return attribute.DocumentType;
            }
            else
            {
                return type;
            }
        }
    }
}
