using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// アーカイブフォルダコレクション
    /// </summary>
    public class FolderArchiveCollection : FolderCollection
    {
        // Fields

        private readonly ArchiveEntryCollectionMode _mode;
        private ArchiveEntryCollection? _collection;


        // Constructors

        public FolderArchiveCollection(QueryPath path, ArchiveEntryCollectionMode mode, bool isActive, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
            _mode = mode;
        }

        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                _collection = new ArchiveEntryCollection(this.Place.SimplePath, ArchiveEntryCollectionMode.CurrentDirectory, _mode, ArchiveEntryCollectionOption.None, ArchiveHint.None);
            }
            catch
            {
                this.Items = new ObservableCollection<FolderItem>() { _folderItemFactory.CreateFolderItemEmpty() };
                return;
            }

            List<ArchiveEntry> entries;
            switch (_mode)
            {
                case ArchiveEntryCollectionMode.CurrentDirectory:
                    entries = (await _collection.GetEntriesWhereBookAsync(token)).ToArchiveEntryCollection();
                    break;
                case ArchiveEntryCollectionMode.IncludeSubDirectories:
                    entries = (await _collection.GetEntriesWhereSubArchivesAsync(token)).ToArchiveEntryCollection();
                    break;
                default:
                    this.Items = new ObservableCollection<FolderItem>() { _folderItemFactory.CreateFolderItemEmpty() };
                    return;
            }

            var items = entries
                .Select(e => _folderItemFactory.CreateFolderItem(e, _collection.Path))
                .Where(e => e != null)
                .ToList();

            var list = Sort(items, token);

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());
        }


        // Properties

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Normal;


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
    }
}
