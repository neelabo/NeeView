using NeeLaboratory.Collection;
using NeeView.Data;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Linq;

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
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.PageEndAction))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsCancelSlideByMouseMove))));
            section.Children.Add(new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.SlideShowInterval)), new SlideShowInterval(), true));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsPrioritizeTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsWaitAnimation))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsTimerVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsAutoScroll))));

            var commandMap = GetNextPageCommandDictionary();
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.NextPageCommandName), new PropertyMemberElementOptions() { StringMap = commandMap })));

            this.Items = new List<SettingItem>() { section };
        }

        // TODO: CommandTalbe 変更のたびに再生成が必要
        private KeyValuePairList<string, string> GetNextPageCommandDictionary()
        {
            return CommandTable.Current
                .Where(e => IsNextPageCommand(e.Value))
                .OrderBy(e => e.Key)
                .ToKeyValuePairList(e => e.Key, e => e.Value.Text);
        }

        private bool IsNextPageCommand(CommandElement command)
        {
            return command switch
            {
                NextPageCommand => true,
                NextOnePageCommand => true,
                NextScrollPageCommand => true,
                NextSizePageCommand => true,
                _ => false,
            };
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
