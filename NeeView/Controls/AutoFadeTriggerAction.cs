using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// 自動フェイドアニメーション : TriggerAction
    /// </summary>
    public class AutoFadeTriggerAction : TriggerAction<FrameworkElement>
    {
        public TimeSpan DisplayTime
        {
            get { return (TimeSpan)GetValue(DisplayTimeProperty); }
            set { SetValue(DisplayTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayTimeProperty =
            DependencyProperty.Register("DisplayTime", typeof(TimeSpan), typeof(AutoFadeTriggerAction), new PropertyMetadata(TimeSpan.FromSeconds(1.0)));


        public Duration FadeTime
        {
            get { return (Duration)GetValue(FadeTimeProperty); }
            set { SetValue(FadeTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FadeTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FadeTimeProperty =
            DependencyProperty.Register("FadeTime", typeof(Duration), typeof(AutoFadeTriggerAction), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(0.5))));


        //
        protected override void Invoke(object parameter)
        {
            AutoFade(AssociatedObject, DisplayTime, FadeTime);
        }


        /// <summary>
        /// UI要素を自動的にフェイドアウトさせる
        /// </summary>
        /// <param name="element">UI要素</param>
        /// <param name="beginTime">フェイド開始時間(秒)</param>
        /// <param name="fadeTime">フェイドアウト時間(秒)</param>
        private static void AutoFade(UIElement element, TimeSpan beginTime, Duration fadeTime)
        {
            // 既存のアニメーションを削除
            element.ApplyAnimationClock(UIElement.OpacityProperty, null);

            // 不透明度を1.0にする
            element.Opacity = 1.0;

            // 不透明度を0.0にするアニメを開始
            var ani = new DoubleAnimation(1, 0, fadeTime) { BeginTime = beginTime };
            element.BeginAnimation(UIElement.OpacityProperty, ani);
        }
    }

}
