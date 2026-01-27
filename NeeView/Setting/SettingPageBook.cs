using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Book
    /// </summary>
    public class SettingPageBook : SettingPage
    {
        public SettingPageBook() : base(TextResources.GetString("SettingPage.Book"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageBookPageSetting(),
                new SettingPageBookMove(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Book.General"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.ArchiveRecursiveMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.BookPageCollectMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsOpenBookAtCurrentPlace))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ExcludeRegexes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Book.ExcludeRegexes, AddDialogHeader = TextResources.GetString("AddParameterDialog.ExclusionPattern"), DefaultCollection = BookConfig.DefaultExcludeRegexes, IsEditable = true, IsRegexRuleEnabled = true }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.BookThumbnailFileName))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.BookThumbnailDepth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.FrameSpace))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.WideRatio))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.DividePageRate))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.FolderSortOrder))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ResetPageWhenRandomSort))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.LoadingPageColor))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Book.WidePage"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ContentsSpace))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.WidePageStretch))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.WidePageVerticalAlignment))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsStaticWidePage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsInsertDummyPage))));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsInsertDummyFirstPage)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Book, nameof(BookConfig.IsInsertDummyPage)),
            });
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsInsertDummyLastPage)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Book, nameof(BookConfig.IsInsertDummyPage)),
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.DummyPageColor))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// SettingPage: BookMove
    /// </summary>
    public class SettingPageBookMove : SettingPage
    {
        public SettingPageBookMove() : base(TextResources.GetString("SettingPage.Book.Move"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Book.MoveBook"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsPrioritizeBookMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsCruise))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Book.MovePage"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsPrioritizePageMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsReadyToPageMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.PageEndAction))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.ResetNextBookPage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsNotifyPageLoop))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.TerminalSound))));

            this.Items.Add(section);
        }
    }

    /// <summary>
    /// SettingPage: BookPageSetting
    /// </summary>
    public class SettingPageBookPageSetting : SettingPage
    {
        public SettingPageBookPageSetting() : base(TextResources.GetString("SettingPage.Book.PageSetting"))
        {
            this.Items = new List<SettingItem>();

            var defaultSetting = Config.Current.BookSettingDefault;
            var settingPolicy = Config.Current.BookSettingPolicy;

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Book.PageSetting"), TextResources.GetString("SettingPage.Book.PageSetting.Remarks"));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.Page)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.Page)))
            {
                Content1 = TextResources.GetString("Word.FirstPage"),
            });

            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.SortMode)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.SortMode))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.PageMode)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.PageMode))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.BookReadOrder)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.BookReadOrder))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.AutoRotate)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.AutoRotate))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.IsSupportedDividePage)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.IsSupportedDividePage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.IsSupportedWidePage)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.IsSupportedWidePage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.IsSupportedSingleFirstPage)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.IsSupportedSingleFirstPage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.IsSupportedSingleLastPage)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.IsSupportedSingleLastPage))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.IsRecursiveFolder)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.IsRecursiveFolder))));
            section.Children.Add(new SettingItemMultiProperty(
                PropertyMemberElement.Create(defaultSetting, nameof(BookSettingConfig.BaseScale)),
                PropertyMemberElement.Create(settingPolicy, nameof(BookSettingPolicyConfig.BaseScale))));

            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Book.SubFolder"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsConfirmRecursive))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Book, nameof(BookConfig.IsAutoRecursive))));
            this.Items.Add(section);
        }

    }


}
