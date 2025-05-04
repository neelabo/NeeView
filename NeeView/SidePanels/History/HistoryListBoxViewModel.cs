using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class HistoryListBoxViewModel : INotifyPropertyChanged
    {
        private readonly HistoryList _model;
        private readonly PanelThumbnailItemSize _thumbnailItemSize;

        public HistoryListBoxViewModel(HistoryList model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(HistoryList.SelectedItem),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));

            _thumbnailItemSize = new PanelThumbnailItemSize(Config.Current.Panels.ThumbnailItemProfile, 5.0 + 1.0, 4.0 + 1.0, new Size(18.0, 18.0));
            _thumbnailItemSize.AddPropertyChanged(nameof(PanelThumbnailItemSize.ItemSize), (s, e) => RaisePropertyChanged(nameof(ThumbnailItemSize)));
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsThumbnailVisible => _model.IsThumbnailVisible;

        public Size ThumbnailItemSize => _thumbnailItemSize.ItemSize;

        public CollectionViewSource CollectionViewSource => _model.CollectionViewSource;

        public BookHistory? SelectedItem
        {
            get => _model.SelectedItem;
            set => _model.SelectedItem = value;
        }


        public void Remove(IEnumerable<BookHistory> items)
        {
            _model.Remove(items);
        }

        public void Load(string path)
        {
            Load(path, ArchiveHint.None);
        }

        public void Load(string path, ArchiveHint archiveHint)
        {
            if (path == null) return;
            BookHub.Current?.RequestLoad(this, path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true, archiveHint);
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }

        public List<BookHistory> GetViewItems()
        {
            var collectionView = (CollectionView)CollectionViewSource.View;
            if (collectionView.NeedsRefresh)
            {
                collectionView.Refresh();
            }
            return collectionView.Cast<BookHistory>().ToList();
        }
    }
}
