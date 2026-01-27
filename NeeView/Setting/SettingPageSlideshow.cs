using NeeView.Data;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Slideshow
    /// </summary>
    public class SettingPageSlideshow : SettingPage
    {
        public SettingPageSlideshow() : base(TextResources.GetString("SettingPage.Slideshow"))
        {
            var section = new SettingItemSection(TextResources.GetString("SettingPage.Slideshow"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsSlideShowByLoop))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsCancelSlideByMouseMove))));
            section.Children.Add(new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.SlideShowInterval)), new SlideShowInterval(), true));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsPrioritizeTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsTimerVisible))));

            this.Items = new List<SettingItem>() { section };
        }

        #region IndexValue

        /// <summary>
        /// スライドショー インターバルテーブル
        /// </summary>
        public class SlideShowInterval : IndexDoubleValue
        {
            private static readonly List<double> _values = new()
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240, 300
            };

            public SlideShowInterval() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public SlideShowInterval(double value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            protected override string GetValueString(double value)
            {
                return $"{value} {TextResources.GetString("Word.Sec")}";
            }
        }

        #endregion
    }
}
