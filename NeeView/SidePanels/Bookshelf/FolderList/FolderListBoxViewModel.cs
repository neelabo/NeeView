using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public partial class FolderListBoxViewModel : ObservableObject
    {
        private readonly FolderList _model;
        private readonly PanelThumbnailItemSize _thumbnailItemSize;


        public FolderListBoxViewModel(FolderList folderList)
        {
            _model = folderList;

            _model.BusyChanged +=
                (s, e) => AppDispatcher.Invoke(() => BusyChanged?.Invoke(s, e));

            _model.PropertyChanged +=
                (s, e) => AppDispatcher.Invoke(() => Model_PropertyChanged(s, e));

            _model.SelectedChanging +=
                (s, e) => AppDispatcher.Invoke(() => Model_SelectedChanging(s, e));

            _model.SelectedChanged +=
                (s, e) => AppDispatcher.Invoke(() => Model_SelectedChanged(s, e));

            _thumbnailItemSize = new PanelThumbnailItemSize(Config.Current.Panels.ThumbnailItemProfile, 5.0 + 1.0, 4.0 + 1.0, new Size(18.0, 18.0));
            _thumbnailItemSize.SubscribePropertyChanged(nameof(_thumbnailItemSize.ItemSize), (s, e) => OnPropertyChanged(nameof(ThumbnailItemSize)));

            DetailToolTip = new PanelListItemDetailToolTip(folderList.FolderListConfig);
        }


        public event EventHandler<ReferenceCounterChangedEventArgs>? BusyChanged;
        public event EventHandler<FolderListSelectedChangedEventArgs>? SelectedChanging;
        public event EventHandler<FolderListSelectedChangedEventArgs>? SelectedChanged;


        public FolderCollection? FolderCollection => _model.FolderCollection;

        public FolderOrder FolderOrder => _model.FolderCollection?.FolderOrder ?? FolderOrder.FileName;

        public bool IsFocusAtOnce
        {
            get => _model.IsFocusAtOnce;
            set => _model.IsFocusAtOnce = value;
        }

        public FolderList Model => _model;

        // サムネイルが表示されている？
        public bool IsThumbnailVisible => _model.IsThumbnailVisible;

        public Size ThumbnailItemSize => _thumbnailItemSize.ItemSize;

        public PanelListItemDetailToolTip DetailToolTip { get; }

        public bool SyncBookOnRename => _model.SyncBookOnRename;

        public PanelListItemProfile PanelListItemProfile
        {
            get
            {
                return _model.FolderListConfig.PanelListItemStyle switch
                {
                    PanelListItemStyle.Normal => Config.Current.Panels.NormalItemProfile,
                    PanelListItemStyle.Content => Config.Current.Panels.ContentItemProfile,
                    PanelListItemStyle.Banner => Config.Current.Panels.BannerItemProfile,
                    PanelListItemStyle.Thumbnail => Config.Current.Panels.ThumbnailItemProfile,
                    _ => throw new InvalidOperationException($"Unsupported  PanelListItemStyle: {_model.FolderListConfig.PanelListItemStyle}"),
                };
            }
        }

        [RelayCommand]
        private void ToggleFolderRecursive()
        {
            _model.ToggleFolderRecursive();
        }

        [RelayCommand]
        private void NewFolder()
        {
            _model.NewFolder();
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    OnPropertyChanged("");
                    break;

                case nameof(FolderList.FolderCollection):
                    OnPropertyChanged(nameof(FolderCollection));
                    OnPropertyChanged(nameof(FolderOrder));
                    break;

                case nameof(FolderList.IsFocusAtOnce):
                    OnPropertyChanged(nameof(IsFocusAtOnce));
                    break;
            }
        }

        private void Model_SelectedChanging(object? sender, FolderListSelectedChangedEventArgs e)
        {
            SelectedChanging?.Invoke(sender, e);
        }

        private void Model_SelectedChanged(object? sender, FolderListSelectedChangedEventArgs e)
        {
            SelectedChanged?.Invoke(sender, e);
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled && _model.FolderListConfig.PanelListItemStyle != PanelListItemStyle.Thumbnail;
        }

        public void MoveToHome()
        {
            _model.MoveToHome();
        }

        public void MoveToUp()
        {
            _model.MoveToParent();
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="item"></param>
        public void MoveToSafety(FolderItem? item)
        {
            if (item != null && item.CanOpenFolder())
            {
                _model.MoveTo(item.TargetPath);
            }
        }

        public void MoveToPrevious()
        {
            _model.MoveToPrevious();
        }

        public void MoveToNext()
        {
            _model.MoveToNext();
        }

        public void IsVisibleChanged(bool isVisible)
        {
            _model.IsVisibleChanged(isVisible);
        }

        public async Task RemoveAsync(IEnumerable<FolderItem> items)
        {
            if (items == null) return;

            await Model.RemoveAsync(items);
        }
    }
}
