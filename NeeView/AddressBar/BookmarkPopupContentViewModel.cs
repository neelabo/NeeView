using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NeeView
{
    public class BookmarkPopupContentViewModel : BindableBase
    {
        private readonly string _bookPath;
        private readonly BookmarkPopupEdit _edit;
        private readonly RootFolderTree _root;
        private readonly BookmarkPopupComboBoxCollection _comboBoxCollection;
        private bool _isTreeViewVisible;

        public BookmarkPopupContentViewModel(string path)
        {
            _bookPath = path;

            _root = new RootFolderTree();
            _root.Children = [new RootBookmarkFolderNode(_root) { IsExpanded = true }];

            _comboBoxCollection = new BookmarkPopupComboBoxCollection(_root, _bookPath);
            _comboBoxCollection.SubscribePropertyChanged(nameof(_comboBoxCollection.SelectedItem), ComboBox_SelectedItemChanged);

            _edit = new(_bookPath, GetBookmarkEntry(_comboBoxCollection.SelectedItem));
        }


        public event EventHandler? FolderTreeViewSelectionChanged;


        public string BookPath => _bookPath;

        public ObservableCollection<FolderTreeNodeBase>? TreeViewItems => _root?.Children;

        public ObservableCollection<FolderTreeNodeBase>? ComboBoxItems => _comboBoxCollection.Items;

        public FolderTreeNodeBase? SelectedItem
        {
            get { return _comboBoxCollection.SelectedItem; }
            set { _comboBoxCollection.SelectedItem = value; }
        }

        public bool IsTreeViewVisible
        {
            get { return _isTreeViewVisible; }
            set { SetProperty(ref _isTreeViewVisible, value); }
        }

        public bool IsEdit => _edit.IsEdit;

        public string TitleText => ResourceService.GetString("@Word.Bookmark");
        public string OKButtonText => ResourceService.GetString("@Word.Add");
        public string DeleteButtonText => ResourceService.GetString("@Word.Remove");


        private void ComboBox_SelectedItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectedItem));

            if (_isTreeViewVisible)
            {
                SetTreeViewSelectedItem(SelectedItem, true);
            }
        }

        public void ApplyBookmark(bool? result)
        {
            _edit.Apply(GetBookmarkEntry(SelectedItem), result);
        }


        public void SetTreeViewSelectedItem(FolderTreeNodeBase? item, bool raiseChangedEvent)
        {
            if (item is null) return;
            if (item.IsSelected) return;

            item.IsExpanded = true;
            item.IsSelected = true;

            if (raiseChangedEvent)
            {
                FolderTreeViewSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private static TreeListNode<IBookmarkEntry>? GetBookmarkEntry(FolderTreeNodeBase? item)
        {
            return (item as BookmarkFolderNode)?.BookmarkSource;
        }
    }

}
