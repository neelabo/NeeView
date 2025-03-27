using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeeLaboratory.ComponentModel
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 汎用SWAP
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            (rhs, lhs) = (lhs, rhs);
        }

        /// <summary>
        /// Deep Copy (by JSON)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T source)
        {
            var options = new JsonSerializerOptions() { IgnoreReadOnlyProperties = true };
            ReadOnlySpan<byte> json =  JsonSerializer.SerializeToUtf8Bytes(source, options);
            return JsonSerializer.Deserialize<T>(json, options) ?? throw new InvalidOperationException("serialize must be succeed");
        }

        /// <summary>
        /// DefaultValue属性でプロパティを初期化する
        /// from: https://stackoverflow.com/questions/2329868/net-defaultvalue-attribute
        /// </summary>
        /// <param name="obj"></param>
        public static void InitializePropertyDefaultValues(this object obj)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (d != null)
                {
                    prop.SetValue(obj, d.Value);
                }
            }
        }

        /// <summary>
        /// Creates the GetHashCode() method. public properties only.
        /// from https://www.brad-smith.info/blog/archives/385
        /// NOTE: Simple. not completed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object, int> MakeGetHashCodeMethod(Type type)
        {
            ParameterExpression pThis = Expression.Parameter(typeof(object), "x");
            UnaryExpression pCastThis = Expression.Convert(pThis, type);

            Expression? last = null;
            foreach (PropertyInfo property in type.GetProperties())
            {
                MemberExpression thisProperty = Expression.Property(pCastThis, property);
                MethodCallExpression getHashCode = Expression.Call(thisProperty, "GetHashCode", Type.EmptyTypes);
                Expression hash;

                if (property.PropertyType.IsClass)
                {
                    // if null, magic number 17
                    BinaryExpression test = Expression.Equal(thisProperty, Expression.Constant(null, property.PropertyType));
                    ConstantExpression ifTrue = Expression.Constant(17, typeof(int));
                    MethodCallExpression ifFalse = getHashCode;
                    hash = Expression.Condition(test, ifTrue, ifFalse);
                }
                else
                {
                    hash = getHashCode;
                }

                if (last == null)
                    last = hash;
                else
                    last = Expression.ExclusiveOr(last, hash);
            }

            if (last is null) throw new InvalidOperationException($"{type.Name} must have any properties");

            return Expression.Lambda<Func<object, int>>(last, pThis).Compile();
        }

        /// <summary>
        /// Creates the Equals() method. public properties only.
        /// from https://www.brad-smith.info/blog/archives/385
        /// NOTE: Simple. not completed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object, object, bool> MakeEqualsMethod(Type type)
        {
            ParameterExpression pThis = Expression.Parameter(typeof(object), "x");
            ParameterExpression pThat = Expression.Parameter(typeof(object), "y");

            // cast to the subclass type
            UnaryExpression pCastThis = Expression.Convert(pThis, type);
            UnaryExpression pCastThat = Expression.Convert(pThat, type);

            // compound AND expression using short-circuit evaluation
            Expression? last = null;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.GetCustomAttribute(typeof(EqualsIgnoreAttribute)) != null)
                    continue;

                BinaryExpression equals = Expression.Equal(
                    Expression.Property(pCastThis, property),
                    Expression.Property(pCastThat, property)
                );

                if (last == null)
                    last = equals;
                else
                    last = Expression.AndAlso(last, equals);
            }

            if (last is null) throw new InvalidOperationException($"{type.Name} must have any properties");

            // call Object.Equals if second parameter doesn't match type
            last = Expression.Condition(
                Expression.TypeIs(pThat, type),
                last,
                Expression.Equal(pThis, pThat)
            );

            // compile method
            return Expression.Lambda<Func<object, object, bool>>(last, pThis, pThat).Compile();
        }
    }

    /// <summary>
    /// MakeEqualsMethod の対象外とする属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EqualsIgnoreAttribute : Attribute
    {
    }
}
