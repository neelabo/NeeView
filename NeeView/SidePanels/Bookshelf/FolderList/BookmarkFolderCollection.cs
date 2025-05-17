﻿using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.IO;
using NeeLaboratory.Linq;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.Generic;

namespace NeeView
{
    public class BookmarkFolderCollection : FolderCollection, IDisposable
    {
        private TreeListNode<IBookmarkEntry> _bookmarkPlace = CreateBookmarkPlaceEmpty();


        public BookmarkFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
        }

        public override async ValueTask InitializeItemsAsync(CancellationToken token)
        {
            await Task.Run(() => InitializeItems(token), token);
        }

        public void InitializeItems(CancellationToken token)
        {
            ThrowIfDisposed();

            _bookmarkPlace = BookmarkCollection.Current.FindNode(Place.FullPath) ?? CreateBookmarkPlaceEmpty();

            var items = CreateFolderItemCollection(_bookmarkPlace, token);
            var list = Sort(items, token);

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            // 変更監視
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }

        protected virtual List<FolderItem> CreateFolderItemCollection(TreeListNode<IBookmarkEntry> root, CancellationToken token)
        {
            var items = root.Children
                .Select(e => CreateFolderItem(e))
                .WhereNotNull()
                .ToList();

            return items;
        }



        public override bool IsSearchEnabled => true;

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Full;

        public TreeListNode<IBookmarkEntry> BookmarkPlace => _bookmarkPlace;


        private static TreeListNode<IBookmarkEntry> CreateBookmarkPlaceEmpty()
        {
            return new TreeListNode<IBookmarkEntry>(new BookmarkEmpty());
        }

        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case EntryCollectionChangedAction.Add:
                    if (e.Item is null) throw new InvalidOperationException();
                    if (e.Parent == _bookmarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item == null)
                        {
                            item = CreateFolderItem(e.Item);
                            if (item is not null)
                            {
                                AddItem(item);
                            }
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Remove:
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Rename:
                    if (e.Item is null) throw new InvalidOperationException();
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            if (e.Item.Value is BookmarkFolder bookmarkFolder)
                            {
                                RenameItem(item, e.Item.CreateQuery());
                            }
                            else if (e.Item.Value is Bookmark bookmark)
                            {
                                RenameItem(item, new QueryPath(bookmark.Path));
                            }
                        }
                    }
                    break;


                case EntryCollectionChangedAction.Move:

                    if (e.Item is null) throw new InvalidOperationException();

                    // 並びが登録順の場合のみ反映
                    if (FolderOrder.IsEntryCategory())
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        var target = Items.FirstOrDefault(i => e.Target == i.Source);
                        if (item != null)
                        {
                            // いろいろここで吸収せんとあかん
                            var oldIndex = Items.IndexOf(item);
                            var newIndex = -1;

                            // e.Target が null ならば、終端に移動。ソート方向に注意。
                            if (e.Target is null)
                            {
                                // Move では常に対象が存在するため、この状態にはならない？
                                newIndex = FolderOrder.IsDescending() ? 0 : Items.Count - 1;
                            }
                            // e.Target が存在し target が null ならば適切な候補を検索する必要あり
                            else if (target is null)
                            {
                                newIndex = Items.Count - 1;
                                for (int i = 0; i < Items.Count; i++)
                                {
                                    if (Items[i] == item) continue;
                                    if (Items[i].Source is TreeListNode<IBookmarkEntry> it)
                                    {
                                        var index = it.GetIndex();
                                        if (index >= e.NewIndex)
                                        {
                                            newIndex = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            // target が存在するならばその位置に移動
                            else
                            {
                                newIndex = Items.IndexOf(target);
                            }

                            if (oldIndex != newIndex && newIndex >= 0)
                            {
                                MoveItem(item, oldIndex, newIndex);
                            }
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Replace:
                case EntryCollectionChangedAction.Reset:
                    // nop. (work at FolderList.)
                    break;
            }
        }


        protected FolderItem? CreateFolderItem(TreeListNode<IBookmarkEntry>? node)
        {
            if (node is null) return null;

            if (node.Value is BookmarkFolder)
            {
                return CreateFolderItemBookmarkFolder(node);
            }
            else if (node.Value is Bookmark)
            {
                return CreateFolderItemBookmark(node);
            }
            else
            {
                return null;
            }
        }

        private FolderItem? CreateFolderItemBookmarkFolder(TreeListNode<IBookmarkEntry> node)
        {
            if (node?.Value is not BookmarkFolder folder) return null;

            return new ConstFolderItem(new FolderThumbnail(), _isOverlayEnabled)
            {
                Source = node,
                Type = FolderItemType.Directory,
                Place = Place,
                Name = folder.Name,
                TargetPath = node.CreateQuery(),
                Length = -1,
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Bookmark,
                IsReady = true
            };
        }

        private static FileSystemInfo? GetFileSystemInfo(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    return directoryInfo;
                }
                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }
            catch
            {
                // アーカイブパス等、ファイル名に使用できない文字が含まれている場合がある
            }
            return null;
        }

        private FolderItem? CreateFolderItemBookmark(TreeListNode<IBookmarkEntry> node)
        {
            if (node?.Value is not Bookmark bookmark) return null;

            var item = new FileFolderItem(_isOverlayEnabled)
            {
                Source = node,
                Type = FolderItemType.File,
                Place = Place,
                Name = bookmark.Name,
                TargetPath = new QueryPath(bookmark.Path),
                Attributes = FolderItemAttribute.Bookmark,
                EntryTime = bookmark.EntryTime,
                IsReady = true
            };

            switch (GetFileSystemInfo(bookmark.Path))
            {
                case DirectoryInfo directoryInfo:
                    item.Length = -1;
                    item.CreationTime = directoryInfo.GetSafeCreationTime();
                    item.LastWriteTime = directoryInfo.GetSafeLastWriteTime();
                    break;
                case FileInfo fileInfo:
                    item.Length = fileInfo.Length;
                    item.CreationTime = fileInfo.GetSafeCreationTime();
                    item.LastWriteTime = fileInfo.GetSafeLastWriteTime();
                    break;
            }

            return item;
        }

        protected override List<FolderItem> Sort(IEnumerable<FolderItem> source, FolderOrder folderOrder, CancellationToken token)
        {
            return folderOrder switch
            {
                FolderOrder.EntryTime
                    => source.OrderBy(e => GetIndex(e)).ToList(),
                FolderOrder.EntryTimeDescending
                    => source.OrderBy(e => GetIndex(e)).Reverse().ToList(),
                _
                    => base.Sort(source, folderOrder, token)
            };

            static int GetIndex(FolderItem item)
            {
                return item.Source is TreeListNode<IBookmarkEntry> node ? node.GetIndex() : 0;
            }
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    BookmarkCollection.Current.BookmarkChanged -= BookmarkCollection_BookmarkChanged;
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
