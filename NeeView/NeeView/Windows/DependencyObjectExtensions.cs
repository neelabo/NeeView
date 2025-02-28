using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView.Windows
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// DependencyProperty 購読
        /// </summary>
        /// <remarks>
        /// メモリーリークするので必ず Dispose すること
        /// </remarks>
        /// <param name="target">対象オブジェクト</param>
        /// <param name="property">購読するプロパティ</param>
        /// <param name="handler">値が変化した時に呼ばれるイベントハンドラ</param>
        /// <returns>Disposable object</returns>
        public static IDisposable SubscribeDependencyProperty(this DependencyObject target, DependencyProperty property, EventHandler handler)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
            if (dpd is null) throw new InvalidOperationException($"Cannot create DependencyPropertyDescriptor: {property} in {target}");

            dpd.AddValueChanged(target, handler);
            return new AnonymousDisposable(() => dpd.RemoveValueChanged(target, handler));
        }
    }
}
