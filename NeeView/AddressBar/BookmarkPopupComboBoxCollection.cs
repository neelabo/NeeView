using NeeLaboratory.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView
{
    public class BookmarkPopupComboBoxCollection : BindableBase
    {
        const int _maxItemCount = 8;

        private readonly RootFolderTree _root;
        private readonly ObservableCollection<FolderTreeNodeBase> _items;
        private FolderTreeNodeBase? _selectedItem;

        public BookmarkPopupComboBoxCollection(RootFolderTree root, string path)
        {
            _root = root;

            // フォルダーをリスト化
            var items = _root.GetExpandedCollection().ToList();
            if (items is null) throw new InvalidOperationException();

            // 登録済みブックマークを最優先に、残りを新しい順に、その先頭部分を選択可能リストとする
            var list = items
                .OrderByDescending(e => ContainsBookmark(e, path))
                .ThenByDescending(e => IsCurrentBookmarkPlace(e))
                .ThenByDescending(e => GetEntryTime(e))
                .Take(_maxItemCount);
            _items = [.. list];

            // 先頭を選択項目とする
            _selectedItem = Items.FirstOrDefault();
        }


        public ObservableCollection<FolderTreeNodeBase> Items => _items;

        public FolderTreeNodeBase? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                EnsureComboBoxItems(value);
                SetProperty(ref _selectedItem, value);
            }
        }


        public void EnsureComboBoxItems(FolderTreeNodeBase? item)
        {
            if (item is null) return;
            if (Items.Contains(item)) return;

            if (Items.Count >= _maxItemCount)
            {
                Items.RemoveAt(Items.Count - 1);
            }
            Items.Add(item);
        }

        private bool ContainsBookmark(FolderTreeNodeBase item, string path)
        {
            if (item is not BookmarkFolderNode folder) return false;
            return folder.ContainsBookmark(path);
        }

        private bool IsCurrentBookmarkPlace(FolderTreeNodeBase item)
        {
            if (item is not BookmarkFolderNode folder) return false;
            return folder.BookmarkSource == BookmarkFolderList.Current.GetBookmarkPlace();
        }

        private DateTime GetEntryTime(FolderTreeNodeBase item)
        {
            if (item is not BookmarkFolderNode folder) return default;
            return folder.GetLastEntryTime();
        }
    }

}
