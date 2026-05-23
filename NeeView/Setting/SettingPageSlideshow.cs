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
        public SettingPageSlideshow() : base(TextResources.GetString("SettingPage.SlideShow"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.SlideShow"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsCancelSlideByMouseMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.SlideShowInterval))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsPrioritizeTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsWaitAnimation))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsTimerVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsAutoScroll))));
            var commandMap = GetNextPageCommandDictionary();
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.NextPageCommandName), new PropertyMemberElementOptions() { StringMap = commandMap })));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.SlideShow.Override"), TextResources.GetString("SettingPage.SlideShow.Override.Remarks"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.PageEndAction))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.PageMoveType))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.PageMoveDuration))));
            this.Items.Add(section);
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

    }
}
