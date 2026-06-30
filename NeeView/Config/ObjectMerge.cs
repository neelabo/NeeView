//#define LOCAL_DEBUG
using NeeLaboratory.Generators;
using NeeView.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public class ObjectMergeOption
    {
        public bool IsIgnoreEnabled { get; set; } = true;
        public Func<Type, PropertyInfo, bool>? IsIgnoreProperty { get; internal set; }
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
                else if (options.IsIgnoreEnabled && options.IsIgnoreProperty?.Invoke(type, property) == true)
                {
                    LocalDebug.WriteLine($"Merge: {property.Name} is ignore by option");
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
                    if (property.PropertyType.GetInterfaces().Contains(typeof(System.Collections.IList)))
                    {
                        // NOTE: かなり限定的な実装になっているので、必要に応じて拡張する
                        var listA1 = property.GetValue(a1) as IList;
                        var listA2 = property.GetValue(a2) as IList;

                        if (listA1 is null || listA2 is null)
                        {
                            throw new NotSupportedException("List instance is null");
                        }

                        var newList = new List<object>();
                        foreach (var item in listA2)
                        {
                            var itemType = item.GetType();
                            var newItem = Activator.CreateInstance(itemType)!;
                            Merge(newItem, item, options);
                            newList.Add(newItem);
                        }

                        if (listA1 is IResetCollection listResetA1)
                        {
                            listResetA1.Reset(newList);
                        }
                        else
                        {
                            listA1.Clear();
                            foreach (var item in newList)
                            {
                                listA1.Add(item);
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    if (v1 == null)
                    {
                        var instanceType = v2 == null ? property.PropertyType : v2.GetType();
                        v1 = Activator.CreateInstance(instanceType);
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
