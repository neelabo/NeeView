using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Slider
    /// </summary>
    public class SettingPageSlider : SettingPage
    {
        public SettingPageSlider() : base(TextResources.GetString("SettingPage.Slider"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageFilmstrip(),
                new SettingPagePageTitle(),
            };

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Slider"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.Thickness))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.Opacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.SliderDirection))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.SliderIndexLayout))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsVisiblePlaylistMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsSyncPageMode))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Filmstrip
    /// </summary>
    public class SettingPageFilmstrip : SettingPage
    {
        public SettingPageFilmstrip() : base(TextResources.GetString("SettingPage.Filmstrip"))
        {
            var section = new SettingItemSection(TextResources.GetString("SettingPage.Filmstrip"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsSliderLinkedFilmStrip))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsVisibleNumber))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsVisiblePlaylistMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsDetailPopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsSelectedCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsManipulationBoundaryFeedbackEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsWheelMovePage))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: PageTitle
    /// </summary>
    public class SettingPagePageTitle : SettingPage
    {
        public SettingPagePageTitle() : base(TextResources.GetString("SettingPage.PageTitle"))
        {
            var section = new SettingItemSection(TextResources.GetString("SettingPage.PageTitle"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.PageTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.PageTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.PageTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemNote(TextResources.GetString("SettingPage.WindowTitle.Note"), TextResources.GetString("SettingPage.WindowTitle.Note.Title")));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
