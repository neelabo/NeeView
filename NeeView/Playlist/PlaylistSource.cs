using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class PlaylistSource
    {
        public static string FormatName => Environment.SolutionName + ".Playlist";

        public PlaylistSource()
        {
            Items = new List<PlaylistSourceItem>();
        }

        public PlaylistSource(IEnumerable<string> items)
        {
            Items = items.Select(e => new PlaylistSourceItem(e)).ToList();
        }

        public PlaylistSource(IEnumerable<PlaylistSourceItem> items)
        {
            Items = items.ToList();
        }

        public FormatVersion Format { get; set; } = new FormatVersion(FormatName);

        public List<PlaylistSourceItem> Items { get; set; }
    }
}
