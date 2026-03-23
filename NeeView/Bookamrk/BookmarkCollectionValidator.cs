using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class BookmarkCollectionValidator
    {
        public static BookmarkCollectionMemento Validate(this BookmarkCollectionMemento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("UserSetting.Format must not be null.");

#pragma warning disable CS0612 // 型またはメンバーが旧型式です

            // ver.42.0
            if (self.Format.CompareTo(new FormatVersion(BookmarkCollectionMemento.FormatName, 42, 0, 6)) < 0)
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
            if (self.Format.CompareTo(new FormatVersion(BookmarkCollectionMemento.FormatName, 44, 0, 0)) < 0)
            {
                // 登録順でソート
                if (self.Nodes is not null)
                {
                    self.Nodes.Children = SortEntryTime(self.Nodes.Children);
                }
            }

            // ver 45.0
            if (self.Format.CompareTo(new FormatVersion(BookmarkCollectionMemento.FormatName, VersionNumber.Ver45_Alpha4)) <= 0)
            {
                // UNCパスの正規化
                if (self.Nodes is not null)
                {
                    foreach (var node in self.Nodes.Walk())
                    {
                        if (node.Path is not null && !node.IsFolder)
                        {
                            node.Path = UncPathTools.ConvertPathToNormalized(node.Path);
                        }
                    }
                }
                if (self.Books is not null)
                {
                    foreach (var book in self.Books)
                    {
                        book.Path = UncPathTools.ConvertPathToNormalized(book.Path);
                    }
                }
            }

            // Obsolete Books (v46.0+)
            if (self.Books is not null && self.Nodes is not null)
            {
                var map = self.Books.ToDictionary(e => e.Path);
                foreach (var item in self.Nodes.Walk())
                {
                    if (!item.IsFolder && item.Path is not null && map.TryGetValue(item.Path, out var book))
                    {
                        item.Page = book.Page;
                        item.Props = book.ToPropertiesString();
                    }
                }
                self.Books = null;
            }

#pragma warning restore CS0612 // 型またはメンバーが旧型式です

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

            return nodes.ThenBy(e => e.EntryTime).ToList();
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
