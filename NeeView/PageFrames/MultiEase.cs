using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 複数の EasingFunction を連続した曲線
    /// </summary>
    /// <remarks>
    /// Add 関数で EasingFunction を追加していき、時間の割合で適用する関数を選択する
    /// </remarks>
    public class MultiEase : EasingFunctionBase
    {
        public MultiEase()
        {
            Items = new();
            EasingMode = EasingMode.EaseIn;
        }

        /// <summary>
        /// EasingFunction メンバー情報。Add 関数によって追加される。
        /// </summary>
        public List<EaseItem> Items
        {
            get { return (List<EaseItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<EaseItem>), typeof(MultiEase), new PropertyMetadata());


        protected override Freezable CreateInstanceCore()
        {
            return new MultiEase();
        }

        /// <summary>
        /// EasingFunction を追加
        /// </summary>
        /// <param name="ease">Easing関数</param>
        /// <param name="s">このEasing関数が処理する距離</param>
        /// <param name="t">このEasing関数が処理する時間</param>
        public void Add(IEasingFunction ease, double s, double t)
        {
            var items = Items;

            if (!items.Any())
            {
                items.Add(new EaseItem(ease, s, t, 0, 0));
            }
            else
            {
                var last = items.Last();
                items.Add(new EaseItem(ease, s, t, last.S1, last.T1));
            }
        }

        protected override double EaseInCore(double normalizedTime)
        {
            var items = Items;

            if (!items.Any()) return normalizedTime;

            var totalS = items.Last().S1;
            var totalT = items.Last().T1;

            if (Math.Abs(totalS) < 0.001)
            {
                //Debug.WriteLine($"{normalizedTime:f2}: [] s={1.0:f2} (no span)");
                return 1.0;
            }

            // select
            var item = items.FirstOrDefault(e => normalizedTime * totalT <= e.T1) ?? items.Last();
            var index = items.IndexOf(item);

            // calc
            var t = (normalizedTime * totalT - item.T0) / (item.T1 - item.T0);
            if (double.IsNaN(t)) t = 1.0;
            var s = (item.S0 + item.Ease.Ease(t) * (item.S1 - item.S0)) / totalS;
            if (double.IsNaN(s)) s = 1.0;
            //Debug.WriteLine($"{normalizedTime:f2}: [{index}] t={t:f2} s={s:f2}");
            return s;
        }
    }




}
