using System;

namespace NeeView
{
    /// <summary>
    /// 高速検索用BookMemento辞書
    /// 履歴、ブックマーク共有の辞書です
    /// SQL使いたくなってきた..
    /// </summary>
    public class BookMementoUnit : IHasPage
    {
        private Page? _archivePage;


        private BookMementoUnit(BookMemento memento)
        {
            Memento = memento;
        }


        public BookMemento Memento { get; set; }

        public string Path => Memento.Path;

        public override string? ToString()
        {
            var s = Memento?.Path;
            return string.IsNullOrEmpty(s) ? base.ToString() : s;
        }

        #region for Thumbnail

        /// <summary>
        /// ArchivePage Property.
        /// サムネイル用
        /// </summary>
        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new Page(new ArchivePageContent(ArchiveEntryUtility.CreateTemporaryEntry(Memento.Path), null));
                    _archivePage.Thumbnail.IsCacheEnabled = true;
                    _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                }
                return _archivePage;
            }
        }

        private void Thumbnail_Touched(object? sender, EventArgs e)
        {
            if (sender is not Thumbnail thumbnail) return;

            BookThumbnailPool.Current.Add(thumbnail);
        }

        #endregion

        #region IHasPage Support

        public Page GetPage()
        {
            return ArchivePage;
        }

        #endregion

        public static BookMementoUnit Create(BookMemento memento)
        {
            return new BookMementoUnit(memento);
        }
    }
}
