// from http://blog.okazuki.jp/entry/20110507/1304738683

namespace NeeView.Windows
{
    using Microsoft.Xaml.Behaviors;
    using System.Windows;
    using TriggerAction = Microsoft.Xaml.Behaviors.TriggerAction;
    using TriggerBase = Microsoft.Xaml.Behaviors.TriggerBase;

    /// <summary>
    /// StyleでBehaviorを設定するために使用するコレクション
    /// </summary>
    public class StyleBehaviorCollection : FreezableCollection<Behavior>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StyleBehaviorCollection();
        }
    }

    /// <summary>
    /// StyleでTriggerを設定するためのコレクション
    /// </summary>
    public class StyleTriggerCollection : FreezableCollection<TriggerBase>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StyleTriggerCollection();
        }
    }

    /// <summary>
    /// Style内でBehaviorを設定するのに使用します。
    /// </summary>
    public static class StyleInteraction
    {
        /// <summary>
        /// Style内でBehaviorを設定します。
        /// </summary>
        public static readonly DependencyProperty BehaviorsProperty =
            DependencyProperty.RegisterAttached(
                "Behaviors",
                typeof(StyleBehaviorCollection),
                typeof(StyleInteraction),
                new PropertyMetadata((sender, e) =>
                {
                    if (e.NewValue == e.OldValue)
                    {
                        return;
                    }

                    if (e.NewValue is not StyleBehaviorCollection styleBehaviors)
                    {
                        return;
                    }

                    // 同じビヘイビアを複数個所に設定できないので、クローンを追加する
                    var behaviors = Interaction.GetBehaviors(sender);
                    foreach (var styleBehavior in styleBehaviors)
                    {
                        behaviors.Add((Behavior)styleBehavior.Clone());
                    }
                }));

        /// <summary>
        /// Style内でTriggerを設定します。
        /// </summary>
        public static readonly DependencyProperty TriggersProperty =
           DependencyProperty.RegisterAttached(
               "Triggers",
               typeof(StyleTriggerCollection),
               typeof(StyleInteraction),
               new PropertyMetadata((sender, e) =>
               {
                   if (e.NewValue == e.OldValue)
                   {
                       return;
                   }

                   if (e.NewValue is not StyleTriggerCollection styleTriggers)
                   {
                       return;
                   }

                   var triggers = Interaction.GetTriggers(sender);
                   foreach (var styleTrigger in styleTriggers)
                   {
                       // TriggerとActionのCloneを作成して追加する
                       var clone = (TriggerBase)styleTrigger.Clone();
                       foreach (var action in styleTrigger.Actions)
                       {
                           clone.Actions.Add((TriggerAction)action.Clone());
                       }

                       triggers.Add(clone);
                   }
               }));

        /// <summary>
        /// Style内でBehaviorを設定するコレクションを取得します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <returns>Behaviorを設定するコレクション</returns>
        public static StyleBehaviorCollection GetBehaviors(DependencyObject obj)
        {
            return (StyleBehaviorCollection)obj.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Style内でBehaviorを設定するコレクションを設定します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <param name="value">設定するコレクション</param>
        public static void SetBehaviors(DependencyObject obj, StyleBehaviorCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        /// <summary>
        /// Style内でTriggerを設定するコレクションを取得します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <returns>Triggerを設定するコレクション</returns>
        public static StyleTriggerCollection GetTriggers(DependencyObject obj)
        {
            return (StyleTriggerCollection)obj.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Style内でTriggerを設定するコレクションを設定します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <param name="value">設定するコレクション</param>
        public static void SetTriggers(DependencyObject obj, StyleTriggerCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }
    }
}
