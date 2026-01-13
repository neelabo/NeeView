using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingPage: MainView
    /// </summary>
    public class SettingPageMainView : SettingPage
    {
        public SettingPageMainView() : base(TextResources.GetString("SettingPage.MainView"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageManipulate(),
                new SettingPageMouse(),
                new SettingPageTouch(),
                new SettingPageLoupe(),
                new SettingPageSlideshow(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.MainView"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.MainViewMargin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.AlternativeContent))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.MainView.MainViewWindow"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsFloatingEndWhenClosed))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsTopmost))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsFrontAsPossible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsHideTitleBar))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsAutoStretch))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsAutoHide))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MainView, nameof(MainViewConfig.IsAutoShow))));
            this.Items.Add(section);
        }
    }

}
