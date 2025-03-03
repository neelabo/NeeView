//#define LOCAL_DEBUG
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public class ObjectMergeOption
    {
        public bool IsIgnoreEnabled { get; set; } = true;
    }

    [LocalDebug]
    public static partial class ObjectMerge
    { 
        /// <summary>
        /// インスタンスのプロパティを上書き
        /// TODO: 配列や辞書の対応
        /// </summary>
        public static void Merge(object a1, object? a2, ObjectMergeOption? options = null)
        {
            ////if (a1 == null && a2 == null) return;
            if (a1 is null || a2 is null) return;

            var type = a1.GetType();
            if (type != a2.GetType()) throw new ArgumentException("a1 must be same type to a2");
            if (!type.IsClass) throw new ArgumentException("a1 must be class");

            options = options ?? new ObjectMergeOption();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var v1 = property.GetValue(a1);
                var v2 = property.GetValue(a2);

                if (v1 == null && v2 == null)
                {
                }
                else if (property.GetCustomAttribute(typeof(ObsoleteAttribute)) != null)
                {
                    LocalDebug.WriteLine($"Merge: {property.Name} is obsolete");
                }
                else if (options.IsIgnoreEnabled && property.GetCustomAttribute(typeof(ObjectMergeIgnoreAttribute)) != null)
                {
                    LocalDebug.WriteLine($"Merge: {property.Name} is ignore");
                }
                else if (property.GetSetMethod(false) == null)
                {
                    LocalDebug.WriteLine($"Merge: {property.Name} is readonly");
                }
                else if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                {
                    property.GetSetMethod(false)?.Invoke(a1, new object?[] { v2 });
                }
                else if (property.GetCustomAttribute(typeof(ObjectMergeReferenceCopyAttribute)) != null || property.PropertyType.GetCustomAttribute(typeof(ObjectMergeReferenceCopyAttribute)) != null)
                {
                    property.GetSetMethod(false)?.Invoke(a1, new object?[] { v2 });
                }
                else if (property.PropertyType.GetInterfaces().Contains(typeof(System.Collections.ICollection)))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (v1 == null)
                    {
                        v1 = Activator.CreateInstance(property.PropertyType);
                        if (v1 is null) throw new InvalidOperationException();
                        property.SetValue(a1, v1);
                    }
                    if (v2 == null)
                    {
                        property.SetValue(a1, v2);
                    }
                    else
                    {
                        Merge(v1, v2, options);
                    }
                }
            }
        }
    }
}
