using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.Resources
{
    public class TextResourceSegmentFactory
    {
        private readonly Dictionary<string, IGrouping<string, KeyValuePair<string, TextResourceItem>>> _groups;

        public TextResourceSegmentFactory(Dictionary<string, TextResourceItem> map)
        {
            _groups = map.GroupBy(e => e.Key.Split('.', 2).First()).ToDictionary(e => e.Key, e => e);
        }

        public TextResourceSegment Create(string prefix)
        {
            if (_groups.TryGetValue(prefix, out var items))
            {
                return new TextResourceSegment(items);
            }
            else
            {
                return new TextResourceSegment();
            }
        }
    }
}
