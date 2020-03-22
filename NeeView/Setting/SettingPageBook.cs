﻿using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class SettingPageBook : SettingPage
    {
        public SettingPageBook() : base(Properties.Resources.SettingPageBook)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageBookVisual(),
                new SettingPageBookPageSetting(),
                new SettingPageBookMove(),
            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookGeneralGeneral,

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Background, nameof(BackgroundConfig.PageBackgroundColor))),
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsOpenbookAtCurrentPlace))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.Excludes)), new SettingItemCollectionControl() { Collection = BookProfile.Current.Excludes, AddDialogHeader=Properties.Resources.WordExcludePath }),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.WideRatio)))),
            };
        }
    }

    public class SettingPageBookVisual : SettingPage
    {
        public SettingPageBookVisual() : base(Properties.Resources.SettingPageBookVisual)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookVisualVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsVisibleBusy))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsIgnoreImageDpi))),
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvas.Current, nameof(ContentCanvas.ContentsSpace)))),
            };
        }
    }

    public class SettingPageBookMove : SettingPage
    {
        public SettingPageBookMove() : base(Properties.Resources.SettingPageBookMove)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookMoveBook,
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.IsCruise)))),

                new SettingItemSection(Properties.Resources.SettingPageBookMovePage,
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsPrioritizePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsMultiplePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current, nameof(BookOperation.PageEndAction))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current, nameof(BookOperation.IsNotifyPageLoop)))),

                new SettingItemSection(Properties.Resources.SettingPageBookMoveAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(SoundPlayerService.Current, nameof(SoundPlayerService.SeCannotMove)))),
            };
        }
    }

    public class SettingPageBookPageSetting : SettingPage
    {
        public SettingPageBookPageSetting() : base(Properties.Resources.SettingPageBookPageSetting)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageBookPageSetting, Properties.Resources.SettingPageBookPageSettingTips,
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.Page)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.Page)))
                    {
                        Content1 = Properties.Resources.WordFirstPage,
                    },
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.SortMode)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.SortMode))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.PageMode)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.PageMode))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.BookReadOrder)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.BookReadOrder))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.IsSupportedDividePage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.IsSupportedDividePage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.IsSupportedWidePage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.IsSupportedWidePage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.IsSupportedSingleFirstPage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.IsSupportedSingleFirstPage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.IsSupportedSingleLastPage)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.IsSupportedSingleLastPage))),
                    new SettingItemMultiProperty(
                            PropertyMemberElement.Create(BookSettingPresenter.Current.DefaultSetting, nameof(BookSetting.IsRecursiveFolder)),
                            PropertyMemberElement.Create(BookSettingPresenter.Current.Generater, nameof(BookSettingGenerater.IsRecursiveFolder)))),

                new SettingItemSection(Properties.Resources.SettingPageBookSubFolder,
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsConfirmRecursive))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsAutoRecursive)))),

                new SettingItemSection(Properties.Resources.SettingPageBookPageSettingAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsSortFileFirst)))),
            };
        }

    }

}
