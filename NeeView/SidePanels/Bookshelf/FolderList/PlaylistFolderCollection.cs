using NeeView.IO;
using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// プレイリスト用フォルダーコレクション
    /// </summary>
    public class PlaylistFolderCollection : FolderCollection
    {
        // Fields

        private ArchiveEntryCollection? _collection;


        // Constructors

        public PlaylistFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
        }

        public override async ValueTask InitializeItemsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _collection = new ArchiveEntryCollection(this.Place.SimplePath, ArchiveEntryCollectionMode.CurrentDirectory, ArchiveEntryCollectionMode.CurrentDirectory, ArchiveEntryCollectionOption.None, ArchiveHint.None);

            var entries = (await _collection.GetEntriesAsync(false, token)).ToArchiveEntryCollection();

            var items = entries
                .Select(e => CreateFolderItem(e, e.Id))
                .WhereNotNull()
                .ToList();

            var list = Sort(items, token);

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
        }


        // Properties

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Full;


        // Methods

        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        public override QueryPath? GetParentQuery()
        {
            if (Place == null)
            {
                return null;
            }
            else if (_collection == null)
            {
                return new QueryPath(ArchiveManager.Current.GetExistPathName(Place.SimplePath));
            }
            else
            {
                return new QueryPath(_collection.GetFolderPlace());
            }
        }


        private FolderItem? CreateFolderItem(ArchiveEntry entry, int id)
        {
            var item = CreateFolderItem(entry);
            if (item != null)
            {
                item.Name = entry.EntryName ?? item.Name;
                item.EntryTime = new DateTime(id);
                item.Attributes |= FolderItemAttribute.PlaylistMember;
            }
            return item;
        }

        private FolderItem? CreateFolderItem(ArchiveEntry entry)
        {
            Debug.Assert(entry is PlaylistArchiveEntry);
            var entity = (entry as PlaylistArchiveEntry)?.InnerEntry ?? throw new InvalidOperationException();

            if (entity.IsFileSystem)
            {
                return _folderItemFactory.CreateFolderItem(entity.SystemPath);
            }
            else
            {
                var archiveType = ArchiveManager.Current.GetSupportedType(entity.EntryLastName);
                if (archiveType != ArchiveType.None)
                {
                    return _folderItemFactory.CreateFolderItem(entity, null);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
