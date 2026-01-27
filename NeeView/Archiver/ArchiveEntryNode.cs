using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntry に親子関係を添付したもの。論理パス。
    /// PlaylistArchive を展開したときに ArchiveEntry だけでは正しい親子関係を取得できないため。
    /// </summary>
    public class ArchiveEntryNode
    {
        public ArchiveEntryNode(ArchiveEntryNode? parent, ArchiveEntry archiveEntry) : this(parent, archiveEntry, "")
        {
        }

        public ArchiveEntryNode(ArchiveEntryNode? parent, ArchiveEntry archiveEntry, string entryPoint)
        {
            Parent = parent;
            ArchiveEntry = archiveEntry;
            EntryPoint = LoosePath.TrimDirectoryEnd(entryPoint);

            Debug.Assert(ArchiveEntry.EntryName.StartsWith(EntryPoint));
        }

        public ArchiveEntryNode? Parent { get; init; }
        public ArchiveEntry ArchiveEntry { get; init; }
        public string EntryPoint { get; init; }


        /// <summary>
        /// ノード名。RootPointを除いたエントリー名。
        /// </summary>
        public string NodeName => ArchiveEntry.EntryName.Substring(EntryPoint.Length);

        /// <summary>
        /// Root のアーカイブパス
        /// </summary>
        public Archive Archive => Parent?.Archive ?? ArchiveEntry.Archive;

        /// <summary>
        /// Root のシステムパス。ブックのパス。
        /// </summary>
        public string RootSystemPath => Parent?.RootSystemPath ?? LoosePath.Combine(ArchiveEntry.Archive.SystemPath, LoosePath.TrimEnd(EntryPoint));

        /// <summary>
        /// Root からの相対パス。ページのエントリー名。
        /// </summary>
        public string EntryName => LoosePath.Combine(Parent?.EntryName, NodeName);
    }
}
