//#define LOCAL_DEBUG

using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class PlaylistEditor : IPlaylistEditor
    {
        private readonly Playlist _playlist;

        public PlaylistEditor(Playlist playlist)
        {
            _playlist = playlist;
        }

        public void Dispose()
        {
        }

        public void Delete(IEnumerable<PlaylistSourceItem> items)
        {
            _playlist.Remove(items.Select(e => GetPlaylistItem(e)).WhereNotNull().ToList());
        }

        public string? Rename(PlaylistSourceItem item, string newName)
        {
            if (item.Name == newName) return null;

            var target = GetPlaylistItem(item);
            if (target is null) return null;
            
            var result = _playlist.Rename(target, newName);
            return target.Name;
        }

        private PlaylistItem? GetPlaylistItem(PlaylistSourceItem item)
        {
            return _playlist.Items.FirstOrDefault(x => x.EqualsTo(item));
        }
    }
}