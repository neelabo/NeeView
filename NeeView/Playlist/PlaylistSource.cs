using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class PlaylistSource
    {
        public static string FormatName => Environment.SolutionName + ".Playlist";
        public static readonly FormatVersion FormatVersion = new(FormatName, 2, 0, 1);

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

        public FormatVersion Format { get; set; } = FormatVersion;

        public List<PlaylistSourceItem> Items { get; set; }
    }
}
