using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class PageListBoxViewModel : BindableBase
    {
        private readonly PageList _model;
        private readonly PanelThumbnailItemSize _thumbnailItemSize;

        public PageListBoxViewModel(PageList model)
        {
            this.CollectionViewSource = new CollectionViewSource();
            this.CollectionViewSource.Culture = TextResources.Culture;

            _model = model;
            _model.CollectionChanged += Model_CollectionChanged;

            _thumbnailItemSize = new PanelThumbnailItemSize(Config.Current.Panels.ThumbnailItemProfile, 5.0 + 1.0, 4.0 + 2.0, new Size(18.0, 18.0));
            _thumbnailItemSize.SubscribePropertyChanged(nameof(_thumbnailItemSize.ItemSize), (s, e) => RaisePropertyChanged(nameof(ThumbnailItemSize)));

            Config.Current.PageList.AddPropertyChanged(nameof(PageListConfig.IsGroupBy),
                (s, e) => UpdateGroupBy());
            UpdateItems();
        }


        public event EventHandler? CollectionChanged;

        public event EventHandler<ViewItemsChangedEventArgs>? ViewItemsChanged;


        public PageList Model => _model;

        /// <summary>
        /// 一度だけフォーカスするフラグ
        /// </summary>
        public bool FocusAtOnce { get; set; }

        public Size ThumbnailItemSize => _thumbnailItemSize.ItemSize;

        public CollectionViewSource CollectionViewSource { get; }

        public bool IsGroupBy => Config.Current.PageList.IsGroupBy;


        private void Model_CollectionChanged(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                UpdateItems();
                CollectionChanged?.Invoke(sender, e);
            });
        }

        private void UpdateItems()
        {
            if (_model is null) return;

            if (CollectionViewSource.Source != _model.Items)
            {
                this.CollectionViewSource.Source = _model.Items;
                UpdateGroupBy();
            }
        }

        private void UpdateGroupBy()
        {
            RaisePropertyChanged(nameof(IsGroupBy));

            this.CollectionViewSource.GroupDescriptions.Clear();
            if (IsGroupBy)
            {
                GroupDescription? groupDescription = _model.PageSortMode switch
                {
                    PageSortMode.FileName or PageSortMode.FileNameDescending
                        => new PageDirectoryGroupDescription(),
                    PageSortMode.Size or PageSortMode.SizeDescending
                        => new PageSizeGroupDescription(),
                    PageSortMode.TimeStamp or PageSortMode.TimeStampDescending
                        => new PageLastWriteTimeGroupDescription(),
                    _
                        => null,
                };

                this.CollectionViewSource.GroupDescriptions.Add(groupDescription);
                this.CollectionViewSource.IsLiveGroupingRequested = groupDescription != null;
            }
        }

        public void Loaded()
        {
            _model.Loaded();
            _model.ViewItemsChanged += (s, e) => ViewItemsChanged?.Invoke(s, e);
        }

        public void Unloaded()
        {
            _model.Unloaded();
            _model.ViewItemsChanged -= (s, e) => ViewItemsChanged?.Invoke(s, e);
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }
    }


    public class PageDirectoryGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is not Page page) return ResourceService.GetString("@Word.ItemNone");

            if (page.PageType != PageType.File)
            {
                return ResourceService.GetString("@Word.Unspecified") + " ";
            }
            else
            {
                return GetDirectoryName(page);
            }
        }

        private static string GetDirectoryName(Page page)
        { 
            var path = LoosePath.GetDirectoryName(page.EntryName) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                // NOTE: 最後にスペースを付けるのは、もしもディレクトリ名が重複したときでもグループを区別させるため
                return ResourceService.GetString("@Word.RootDirectory") + " ";
            }
            else
            {
                return path.Replace("\\", " > ", StringComparison.Ordinal);
            }
        }
    }

    public class PageLastWriteTimeGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is not Page page) return ResourceService.GetString("@Word.ItemNone");

            if (page.PageType != PageType.File)
            {
                return ResourceService.GetString("@Word.Unspecified");
            }
            else
            {
                return GetDateString(page.LastWriteTime.Date, culture);
            }
        }

        private static string GetDateString(DateTime date, CultureInfo culture)
        {
            var today = DateTime.Today;
            if (date == today)
            {
                return ResourceService.GetString("@Word.Today");
            }
            else if (date == today.AddDays(-1))
            {
                return ResourceService.GetString("@Word.Yesterday");
            }
            else
            {
                return date.ToString("D", culture);
            }
        }
    }

    public class PageSizeGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is not Page page) return ResourceService.GetString("@Word.ItemNone");

            if (page.PageType != PageType.File)
            {
                return ResourceService.GetString("@Word.Unspecified");
            }
            else
            {
                return GetSizeString(page.Length, culture);
            }
        }

        private static string GetSizeString(long length, CultureInfo culture)
        {
            if (length < 0)
            {
                return ResourceService.GetString("@Word.Unspecified");
            }
            else if (length == 0)
            {
                return "0 KB";
            }
            else if (length <= 16 * 1024)
            {
                return "0 - 16 KB";
            }
            else if (length <= 1024 * 1024)
            {
                return "16 KB - 1 MB";
            }
            else if (length <= 128 * 1024 * 1024)
            {
                return "1 - 128 MB";
            }
            else if (length <= 1024 * 1024 * 1024)
            {
                return "128 MB - 1 GB";
            }
            else if (length <= 4L * 1024 * 1024 * 1024)
            {
                return "1 - 4 GB";
            }
            else
            {
                return ">4 GB";
            }
        }
    }
}
