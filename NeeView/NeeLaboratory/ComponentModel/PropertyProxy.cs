using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// ターゲットオブジェクトのプロパティに対するプロキシを提供します。
    /// </summary>
    /// <typeparam name="TTarget">ターゲットオブジェクトの型</typeparam>
    /// <typeparam name="TProperty">プロパティの型</typeparam>
    public class PropertyProxy<TTarget, TProperty>
    {
        private readonly TTarget _target;
        private readonly Func<TTarget, TProperty> _getter;
        private readonly Action<TTarget, TProperty> _setter;

        /// <summary>
        /// <see cref="PropertyProxy{TTarget, TProperty}"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="target">プロパティアクセスの対象となるオブジェクト</param>
        /// <param name="propertyExpression">アクセスするプロパティを指定するラムダ式 (例: p => p.Name)</param>
        /// <exception cref="ArgumentException">式がプロパティを参照していない場合、またはメンバーがプロパティではない場合</exception>
        public PropertyProxy(TTarget target, Expression<Func<TTarget, TProperty>> propertyExpression)
        {
            _target = target;

            // 1. Parse and retrieve property information from the expression tree
            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException("The expression must refer to a property (e.g., p => p.Name)");
            }

            var propInfo = member.Member as PropertyInfo ?? throw new ArgumentException("The specified member is not a property.");

            // 2. Compile the getter (p => p.Property)
            _getter = propertyExpression.Compile();

            // 3. Dynamically assemble and compile the setter ((t, v) => t.Property = v)
            var targetParam = Expression.Parameter(typeof(TTarget), "t");
            var valueParam = Expression.Parameter(typeof(TProperty), "v");
            var propertyAccess = Expression.Property(targetParam, propInfo);
            var assign = Expression.Assign(propertyAccess, valueParam);

            _setter = Expression.Lambda<Action<TTarget, TProperty>>(assign, targetParam, valueParam).Compile();
        }

        /// <summary>
        /// プロパティの現在の値を取得します。
        /// </summary>
        /// <returns>プロパティの値</returns>
        public TProperty GetValue() => _getter(_target);

        /// <summary>
        /// プロパティに値を設定します。
        /// </summary>
        /// <param name="value">設定する値</param>
        public void SetValue(TProperty value) => _setter(_target, value);
    }


    /// <summary>
    /// <see cref="PropertyProxy{TTarget, TProperty}"/> を作成するためのヘルパークラスです。
    /// </summary>
    public static class ProxyProperty
    {
        /// <summary>
        /// プロパティプロキシを作成します。
        /// </summary>
        /// <typeparam name="TTarget">ターゲットオブジェクトの型</typeparam>
        /// <typeparam name="TProperty">プロパティの型</typeparam>
        /// <param name="target">プロパティアクセスの対象となるオブジェクト</param>
        /// <param name="expr">アクセスするプロパティを指定するラムダ式</param>
        /// <returns>作成されたプロパティプロキシ</returns>
        public static PropertyProxy<TTarget, TProperty> Create<TTarget, TProperty>(TTarget target, Expression<Func<TTarget, TProperty>> expr)
        {
            return new PropertyProxy<TTarget, TProperty>(target, expr);
        }
    }

}
