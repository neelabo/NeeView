using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class PageListBoxViewModel : BindableBase
    {
        private readonly PageList _model;
        private readonly PanelThumbnailItemSize _thumbnailItemSize;

        public PageListBoxViewModel(PageList model)
        {
            _model = model;
            _model.CollectionChanged += (s, e) =>
                AppDispatcher.Invoke(() => CollectionChanged?.Invoke(s, e));

            _thumbnailItemSize = new PanelThumbnailItemSize(Config.Current.Panels.ThumbnailItemProfile, 5.0 + 1.0, 4.0 + 2.0, new Size(18.0, 18.0));
            _thumbnailItemSize.SubscribePropertyChanged(nameof(_thumbnailItemSize.ItemSize), (s, e) => RaisePropertyChanged(nameof(ThumbnailItemSize)));
        }


        public event EventHandler? CollectionChanged;

        public event EventHandler<ViewItemsChangedEventArgs>? ViewItemsChanged;


        public PageList Model => _model;

        /// <summary>
        /// 一度だけフォーカスするフラグ
        /// </summary>
        public bool FocusAtOnce { get; set; }

        public Size ThumbnailItemSize => _thumbnailItemSize.ItemSize;


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
}
