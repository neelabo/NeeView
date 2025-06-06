using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class BookmarkCollectionValidator
    {
        public static BookmarkCollection.Memento Validate(this BookmarkCollection.Memento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("UserSetting.Format must not be null.");

            // ver.42.0
            if (self.Format.CompareTo(new FormatVersion(BookmarkCollection.Memento.FormatName, 42, 0, 6)) < 0)
            {
                // プレイリストブックのサブフォルダ読み込みを解除
                if (self.Books is not null)
                {
                    foreach (var item in self.Books.Where(e => PlaylistArchive.IsSupportExtension(e.Path)))
                    {
                        item.IsRecursiveFolder = false;
                    }
                }
            }

            // ver 44.0
            if (self.Format.CompareTo(new FormatVersion(BookmarkCollection.Memento.FormatName, 44, 0, 0)) < 0)
            {
                // 登録順でソート
                if (self.Nodes is not null)
                {
                    self.Nodes.Children = SortEntryTime(self.Nodes.Children);
                }
            }

            return self;
        }


        private static List<BookmarkNode>? SortEntryTime(List<BookmarkNode>? source)
        {
            if (source is null) return null;

            foreach (var folder in source.Where(e => e.IsFolder))
            {
                folder.Children = SortEntryTime(folder.Children);
            }

            IOrderedEnumerable<BookmarkNode> nodes = Config.Current.Bookshelf.FolderSortOrder switch
            {
                FolderSortOrder.First
                    => source.OrderBy(e => e.IsFolder),
                FolderSortOrder.Last
                    => source.OrderByDescending(e => e.IsFolder),
                _
                    => source.OrderBy(e => 0),
            };

            return nodes.ThenBy(e => e.EntryTime).ThenBy(e => e, new ComparerTaskNodeName()).ToList();
        }

        private class ComparerTaskNodeName : IComparer<BookmarkNode>
        {
            public int Compare(BookmarkNode? x, BookmarkNode? y)
            {
                return NaturalSort.Compare(x?.Name, y?.Name);
            }
        }
    }
}
