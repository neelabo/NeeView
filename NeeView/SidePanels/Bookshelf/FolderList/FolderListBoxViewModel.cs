using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class FolderListBoxViewModel : BindableBase
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
            _thumbnailItemSize.SubscribePropertyChanged(nameof(_thumbnailItemSize.ItemSize), (s, e) => RaisePropertyChanged(nameof(ThumbnailItemSize)));

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


        #region RelayCommands

        private RelayCommand? _toggleFolderRecursive;
        private RelayCommand? _newFolderCommand;


        public RelayCommand ToggleFolderRecursive
        {
            get { return _toggleFolderRecursive = _toggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive_Executed); }
        }

        // HACK: 未使用？
        public RelayCommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand = _newFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.NewFolder();
                }
            }
        }

        #endregion RelayCommands


        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged(null);
                    break;

                case nameof(FolderList.FolderCollection):
                    RaisePropertyChanged(nameof(FolderCollection));
                    RaisePropertyChanged(nameof(FolderOrder));
                    break;

                case nameof(FolderList.IsFocusAtOnce):
                    RaisePropertyChanged(nameof(IsFocusAtOnce));
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

        public async ValueTask RemoveAsync(IEnumerable<FolderItem> items)
        {
            if (items == null) return;

            await Model.RemoveAsync(items);
        }
    }
}
