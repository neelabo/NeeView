using System;
using System.Linq;

namespace NeeView
{
    public static class BookHistoryCollectionValidator
    {
        public static BookHistoryCollection.Memento Validate(this BookHistoryCollection.Memento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("UserSetting.Format must not be null.");

            // ver.42.0
            if (self.Format.CompareTo(new FormatVersion(BookHistoryCollection.Memento.FormatName, 42, 0, 6)) < 0)
            {
                // プレイリストブックのサブフォルダ読み込みを解除
                foreach (var item in self.Books.Where(e => PlaylistArchive.IsSupportExtension(e.Path)))
                {
                    item.IsRecursiveFolder = false;
                }
            }

            // ver 45.0
            if (self.Format.CompareTo(new FormatVersion(BookHistoryCollection.Memento.FormatName, VersionNumber.Ver45_Alpha4)) <= 0)
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

            return self;
        }
    }
}
