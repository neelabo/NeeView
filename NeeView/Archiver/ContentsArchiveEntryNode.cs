using System.Collections.Generic;

namespace NeeView
{
    public class ContentsArchiveEntryNode
    {
        public ContentsArchiveEntryNode(string title, ArchiveEntry archiveEntry)
        {
            Title = title;
            ArchiveEntry = archiveEntry;
        }

        public string Title { get; set; }
        public ArchiveEntry ArchiveEntry { get; set; }
        public List<ContentsArchiveEntryNode>? Children { get; set; }
    }
}
