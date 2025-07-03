using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView.Setting
{
    public class SettingPagePanels : SettingPage
    {
        /// <summary>
        /// Setting: Panels
        /// </summary>
        public SettingPagePanels() : base(Properties.TextResources.GetString("SettingPage.Panels"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPagePanelItems(),
                new SettingPageBookshelf(),
                new SettingPageHistory(),
                new SettingPageFileInfo(),
                new SettingPageEffect(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Panels.AutoHide"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideFocusLockMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.IsAutoHideKeyDownDelay))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideDelayVisibleTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideDelayTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideHitTestHorizontalMargin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideHitTestVerticalMargin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideConflictTopMargin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideConflictBottomMargin))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Panels.AutoHideMode"));
            section.Children.Add(new SettingItemHeader(Properties.TextResources.GetString("SettingPage.Panels.AutoHideMode.WindowState")));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsAutoHideInFullScreen), new PropertyMemberElementOptions() { Name = AliasNameExtensions.GetAliasName(WindowStateEx.FullScreen) })));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsAutoHideInMaximized), new PropertyMemberElementOptions() { Name = AliasNameExtensions.GetAliasName(WindowStateEx.Maximized) })));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsAutoHideInNormal), new PropertyMemberElementOptions() { Name = AliasNameExtensions.GetAliasName(WindowStateEx.Normal) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHideMenuInAutoHideMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsHideLeftPanelInAutoHideMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsHideRightPanelInAutoHideMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsHidePageSliderInAutoHideMode))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Panels.SidePanels"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.Opacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.OpenWithDoubleClick))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsLeftRightKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsManipulationBoundaryFeedbackEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsLimitPanelWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsVisibleItemsCount))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsTextSearchEnabled))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: PanelItems
    /// </summary>
    public class SettingPagePanelItems : SettingPage
    {
        public SettingPagePanelItems() : base(Properties.TextResources.GetString("SettingPage.PanelItems"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.PanelItems.StyleNormal"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.NormalItemProfile, nameof(PanelListItemProfile.IsDetailPopupEnabled))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.PanelItems.StyleContent"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.ImageShape))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsDetailPopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsDecoratePlace))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.PanelItems.StyleBanner"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsDetailPopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.PanelItems.StyleThumbnail"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.ImageShape))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsDetailPopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsTextVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.MouseWheelSpeedRate))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: Bookshelf
    /// </summary>
    public class SettingPageBookshelf : SettingPage
    {
        public SettingPageBookshelf() : base(Properties.TextResources.GetString("SettingPage.Bookshelf"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Bookshelf.General"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.Home))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsVisibleBookmarkMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsVisibleHistoryMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepFolderStatus))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsCloseBookWhenMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsOpenNextBookWhenRemove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsInsertItem))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsHiddenFileVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsMultipleRarFilterEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.ExcludePattern))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.FolderSortOrder))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.DefaultFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Normal.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.PlaylistFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Full.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Full.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Bookshelf.Tree"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.FolderTreeLayout))));
            //section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FolderTreeFontSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsSyncFolderTree))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: FileInfo
    /// </summary>
    public class SettingPageFileInfo : SettingPage
    {
        public SettingPageFileInfo() : base(Properties.TextResources.GetString("SettingPage.Information"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Information.Visual"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.DateTimeFormat))));
            ////section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.MapProgramFormat))) { IsStretch = true });

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Effect
    /// </summary>
    public class SettingPageEffect : SettingPage
    {
        public SettingPageEffect() : base(Properties.TextResources.GetString("SettingPage.Effect"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Effect.Visual"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.ImageDotKeep, nameof(ImageDotKeepConfig.Threshold))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.ImageEffect, nameof(ImageEffectConfig.IsHsvMode))));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
