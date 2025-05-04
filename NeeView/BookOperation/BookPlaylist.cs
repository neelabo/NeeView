using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class BookPlaylist
    {
        private readonly Book _book;
        private readonly Playlist _playlist;

        public BookPlaylist(Book book, Playlist playlist)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _playlist = playlist;
        }

        public bool CanRegister(Page page)
        {
            if (page is null || page.Content is EmptyPageContent)
            {
                return false;
            }

            if (_playlist is null || !_playlist.IsEditable)
            {
                return false;
            }

            if (_book.IsTemporary)
            {
                return false;
            }

            if (page.ArchiveEntry.Archive.Encrypted)
            {
                return false;
            }

            return true;
        }

        public bool Contains(Page page)
        {
            if (_playlist is null) return false;
            if (page is null) return false;

            return Find(page) != null;
        }

        public PlaylistItem? Find(Page page)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            return _playlist.Find(page.EntryFullName);
        }

        public PlaylistItem? Add(Page page)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            return Add(new List<Page> { page })?.FirstOrDefault();
        }

        public List<PlaylistItem>? Add(IEnumerable<Page> pages)
        {
            if (_playlist is null) return null;
            if (pages is null) return null;

            var paths = pages.Select(e => e.ArchiveEntry.SystemPath).ToList();
            return _playlist.Add(paths);
        }

        public bool Remove(Page page)
        {
            if (_playlist is null) return false;
            if (page is null) return false;

            return Remove(new List<Page> { page });
        }

        public bool Remove(IEnumerable<Page> pages)
        {
            if (_playlist is null) return false;
            if (pages is null) return false;

            var items = _playlist.Collect(pages.Select(e => e.EntryFullName).ToList());
            if (items.Any())
            {
                _playlist.Remove(items);
                return true;
            }
            else
            {
                return false;
            }
        }

        public PlaylistItem? Set(Page page, bool isEntry)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            if (isEntry)
            {
                return Add(page);
            }
            else
            {
                Remove(page);
                return null;
            }
        }

        public PlaylistItem? Toggle(Page page)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            return Set(page, Find(page) is null);
        }

        public List<Page> Collect()
        {
            if (_playlist?.Items is null) return new List<Page>();

            return _playlist.Items
                .Select(e => _book.Pages.PageMap.TryGetValue(e.Path, out Page? page) ? page : null)
                .WhereNotNull()
                .ToList();
        }

    }
}
