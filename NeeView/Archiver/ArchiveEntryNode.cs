namespace NeeView
{
    /// <summary>
    /// ArchiveEntry に親子関係を添付したもの。論理パス。
    /// PlaylistArchive を展開したときに ArchiveEntry だけでは正しい親子関係を取得できないため。
    /// </summary>
    public class ArchiveEntryNode
    {
        public ArchiveEntryNode(ArchiveEntryNode? parent, ArchiveEntry archiveEntry)
        {
            Parent = parent;
            ArchiveEntry = archiveEntry;
        }

        public ArchiveEntryNode? Parent { get; init; }
        public ArchiveEntry ArchiveEntry { get; init; }

        /// <summary>
        /// Root のアーカイブパス。ブックのパス。
        /// </summary>
        public Archive Archive => Parent?.Archive ?? ArchiveEntry.Archive;

        /// <summary>
        /// Root からの相対パス。ページのエントリー名。
        /// </summary>
        public string EntryName => LoosePath.Combine(Parent?.EntryName, ArchiveEntry.EntryName);
    }
}
