using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: History
    /// </summary>
    public class SettingPageHistory : SettingPage
    {
        public SettingPageHistory() : base(TextResources.GetString("SettingPage.History"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.History.General"));
            section.Children.Add(new SettingItemIndexValue<int>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.HistoryEntryPageCount)), new HistoryEntryPageCount(), true));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsInnerArchiveHistoryEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsUncHistoryEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsForceUpdateHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsAutoCleanupEnabled))));
            section.Children.Add(new SettingItemButton(TextResources.GetString("SettingPage.History.GeneralDelete"), TextResources.GetString("SettingPage.History.GeneralDeleteButton"), RemoveHistory));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.History.GeneralLimit"), TextResources.GetString("SettingPage.History.GeneralLimit.Remarks"));
            section.Children.Add(new SettingItemIndexValue<int>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.LimitSize)), new HistoryLimitSize(), true));
            section.Children.Add(new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.LimitSpan)), new HistoryLimitSpan(), true));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.History.RecentBook"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.RecentBookCount))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.History.PageViewRecord"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageViewRecorder, nameof(PageViewRecorderConfig.IsSavePageViewRecord))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageViewRecorder, nameof(PageViewRecorderConfig.PageViewRecordFilePath))) { IsStretch = true });
            this.Items.Add(section);
        }

        #region Commands

        private RelayCommand<UIElement>? _RemoveHistory;
        public RelayCommand<UIElement> RemoveHistory
        {
            get { return _RemoveHistory = _RemoveHistory ?? new RelayCommand<UIElement>(RemoveHistory_Executed); }
        }

        private void RemoveHistory_Executed(UIElement? element)
        {
            BookHistoryCollection.Current.Clear();

            var dialog = new MessageDialog("", TextResources.GetString("HistoryDeletedDialog.Title"));
            if (element != null)
            {
                dialog.Owner = Window.GetWindow(element);
            }
            dialog.ShowDialog();
        }

        #endregion

        #region IndexValues

        /// <summary>
        /// 履歴登録開始テーブル
        /// </summary>
        public class HistoryEntryPageCount : IndexIntValue
        {
            private static readonly List<int> _values = new()
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 50, 100,
            };

            public HistoryEntryPageCount() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public HistoryEntryPageCount(int value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            protected override string GetValueString(int value)
            {
                return $"{value} {TextResources.GetString("Word.Page")}";
            }
        }

        /// <summary>
        /// 履歴サイズテーブル
        /// </summary>
        public class HistoryLimitSize : IndexIntValue
        {
            private static readonly List<int> _values = new()
            {
                0, 1, 10, 20, 50, 100, 200, 500, 1000, -1
            };

            public HistoryLimitSize() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public HistoryLimitSize(int value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            protected override string GetValueString(int value)
            {
                return value == -1 ? TextResources.GetString("Word.NoLimit") : value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// 履歴期限テーブル
        /// </summary>
        public class HistoryLimitSpan : IndexTimeSpanValue
        {
            private static readonly List<TimeSpan> _values = new() {
                TimeSpan.FromDays(1),
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                TimeSpan.FromDays(365),
                default,
            };

            public HistoryLimitSpan() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public HistoryLimitSpan(TimeSpan value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            public override IValueConverter? Converter { get; } = new TimeSpanToDaysStringConverter();

            protected override string GetValueString(TimeSpan value)
            {
                return value == default ? TextResources.GetString("Word.NoLimit") : TextResources.GetFormatString("Word.DaysAgo", value.Days);
            }
        }

        #endregion
    }

}
