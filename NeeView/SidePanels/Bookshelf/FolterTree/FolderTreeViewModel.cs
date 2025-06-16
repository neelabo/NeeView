using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FolderTreeViewModel : BindableBase
    {
        private bool _isFirstVisible;

        public FolderTreeViewModel()
        {
        }


        public event EventHandler? SelectedItemChanged;


        private FolderTreeModel? _model;
        public FolderTreeModel? Model
        {
            get { return _model; }
            set
            {
                if (value != _model)
                {
                    if (_model != null)
                    {
                        _model.SelectedItemChanged -= Model_SelectedItemChanged;
                    }
                    _model = value;
                    if (_model != null)
                    {
                        _model.SelectedItemChanged += Model_SelectedItemChanged;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsValid => _model != null;


        private double _dpiScale = 1.0;
        public double DpiScale
        {
            get { return _dpiScale; }
            set { SetProperty(ref _dpiScale, value); }
        }


        internal void DpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            DpiScale = newDpi.DpiScaleX;
        }

        private void Model_SelectedItemChanged(object? sender, EventArgs e)
        {
            SelectedItemChanged?.Invoke(sender, e);
        }

        public void SelectRootQuickAccess()
        {
            Model?.SelectRootQuickAccess();
        }

        public void Decide(object item)
        {
            Model?.Decide(item);
        }

        public void AddQuickAccess(object item)
        {
            Model?.AddQuickAccess(item);
        }

        public void MoveQuickAccess(QuickAccessNodeBase src, QuickAccessNodeBase dst, int delta)
        {
            Model?.MoveQuickAccess(src, dst, delta);
        }

        public void RemoveQuickAccessFolder(QuickAccessFolderNode item)
        {
            Model?.RemoveQuickAccessFolder(item);
        }

        public void RemoveQuickAccess(QuickAccessNode item)
        {
            Model?.RemoveQuickAccess(item);
        }

        public QuickAccessFolderNode? NewQuickAccessFolder(QuickAccessFolderNode item)
        {
            return Model?.NewQuickAccessFolder(item);
        }

        public BookmarkFolderNode? NewBookmarkFolder(BookmarkFolderNode item)
        {
            return Model?.NewBookmarkFolder(item);
        }

        public void AddBookmarkTo(BookmarkFolderNode item)
        {
            Model?.AddBookmarkTo(item);
        }

        public void RemoveBookmarkFolder(BookmarkFolderNode item)
        {
            Model?.RemoveBookmarkFolder(item);
        }

        public void RefreshFolder()
        {
            Model?.RefreshDirectory();
        }

        public void IsVisibleChanged(bool isVisible)
        {
            if (isVisible && !_isFirstVisible)
            {
                _isFirstVisible = true;
                Model?.ExpandRoot();
            }
        }

    }
}
