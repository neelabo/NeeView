using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class BookmarkPopupContentViewModel : ObservableObject
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

            TreeListNode<IBookmarkEntry>? node = null;
            if (BookmarkFolderList.Current.SelectedItem is BookmarkFolderItem item && item.Bookmark.Path == path)
            {
                node = item.BookmarkNode;
            }

            _comboBoxCollection = new BookmarkPopupComboBoxCollection(_root, _bookPath);
            _comboBoxCollection.SubscribePropertyChanged(nameof(_comboBoxCollection.SelectedItem), ComboBox_SelectedItemChanged);
            Debug.Assert(node is null || node.Parent == GetBookmarkEntry(_comboBoxCollection.SelectedItem));

            _edit = new(_bookPath, GetBookmarkEntry(_comboBoxCollection.SelectedItem), node);
            _edit.SubscribePropertyChanged(nameof(_edit.Name), (s, e) => OnPropertyChanged(nameof(Name)));

            if (_edit.IsEdit)
            {
                TitleText = TextResources.GetString("BookmarkDialog.Edit");
                IsAddButtonVisible = true;
            }
            else
            {
                TitleText = TextResources.GetString("BookmarkDialog.Add");
                IsAddButtonVisible = false;
            }

            Tags = CreateTags();
        }


        public event EventHandler? FolderTreeViewSelectionChanged;


        public string Name
        {
            get => _edit.Name;
            set => _edit.Name = value;
        }


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

        public string TitleText { get; }
        public string OKButtonText => TextResources.GetString("Word.Done");
        public string AddButtonText => TextResources.GetString("Word.Add");
        public string DeleteButtonText => TextResources.GetString("Word.Remove");
        public bool IsAddButtonVisible { get; }

        public List<TagItem> Tags { get; }


        private List<TagItem> CreateTags()
        {
            var entries = BookmarkCollection.Current.Collect(_bookPath);
            return entries
                .Where(e => e is not null && e.Parent != null && e.Parent != BookmarkCollection.Current.Items)
                .Distinct()
                .Select(e => new TagItem(e.Parent!, e))
                .ToList();
        }

        private void ComboBox_SelectedItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectedItem));

            if (_isTreeViewVisible)
            {
                SetTreeViewSelectedItem(SelectedItem, true);
            }
        }

        public void ApplyBookmark(BookmarkPopupResult result)
        {
            var folder = GetBookmarkEntry(SelectedItem);
            switch (result)
            {
                case BookmarkPopupResult.Add:
                    _edit.Add(folder);
                    break;
                case BookmarkPopupResult.Remove:
                    _edit.Remove(folder);
                    break;
                case BookmarkPopupResult.Edit:
                    _edit.Edit(folder);
                    break;
                case BookmarkPopupResult.None:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
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


    public enum BookmarkPopupResult
    {
        None,
        Add,
        Remove,
        Edit,
    }
}
