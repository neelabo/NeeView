using System;
using System.Collections;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryTree用ノード
    /// </summary>
    public class ArchiveEntryTreeNode : IEnumerable<ArchiveEntryTreeNode>
    {
        public ArchiveEntryTreeNode()
        {
            Parent = null;
            Name = "";
        }

        public ArchiveEntryTreeNode(ArchiveEntryTreeNode parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        public string Name { get; private set; }

        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public ArchiveEntryTreeNode? Parent { get; private set; }
        public List<ArchiveEntryTreeNode> Children { get; private set; } = new List<ArchiveEntryTreeNode>();

        public string Path => LoosePath.Combine(Parent?.Path, Name);

        /// <summary>
        /// ArchiveEntryが存在する場合の入れ物
        /// </summary>
        public ArchiveEntry? ArchiveEntry { get; set; }


        public IEnumerator<ArchiveEntryTreeNode> GetEnumerator()
        {
            yield return this;
            foreach (var child in Children)
            {
                foreach (var subChild in child)
                {
                    yield return subChild;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
