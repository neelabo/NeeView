using System.Collections.Generic;

namespace NeeLaboratory.Resources
{
    public class TextResourceSegment : Dictionary<string, TextResourceItem>
    {
        public TextResourceSegment()
        {
        }

        public TextResourceSegment(IEnumerable<KeyValuePair<string, TextResourceItem>> items) : base(items)
        {
        }
    }
}
