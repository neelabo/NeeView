using System;
using System.Linq;

namespace NeeView
{
    public static class BookHistoryCollectionValidator
    {
        public static BookHistoryCollectionMemento Validate(this BookHistoryCollectionMemento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("UserSetting.Format must not be null.");

#pragma warning disable CS0612 // 型またはメンバーが旧型式です

            // ver.42.0
            if (self.Format.CompareTo(new FormatVersion(BookHistoryCollectionMemento.FormatName, 42, 0, 6)) < 0)
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

            // ver 45.0
            if (self.Format.CompareTo(new FormatVersion(BookHistoryCollectionMemento.FormatName, VersionNumber.Ver45_Alpha4)) <= 0)
            {
                // UNCパスの正規化。これにより重複したものは削除
                if (self.Items is not null)
                {
                    foreach (var item in self.Items)
                    {
                        if (item.Path is not null)
                        {
                            item.Path = UncPathTools.ConvertPathToNormalized(item.Path);
                        }
                    }
                    self.Items = self.Items.DistinctBy(e => e.Path).ToList();
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
            if (self.Books is not null && self.Items is not null)
            {
                var map = self.Books.ToDictionary(e => e.Path);
                foreach (var item in self.Items)
                {
                    if (map.TryGetValue(item.Path, out var book))
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
    }
}
