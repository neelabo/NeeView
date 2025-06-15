using System;

namespace NeeView
{
    public static class FolderNodeAccessorFactory
    {
        public static NodeAccessor Create(FolderTreeModel model, FolderTreeNodeBase node)
        {
            return node switch
            {
                RootQuickAccessNode n
                    => new QuickAccessFolderNodeAccessor(model, n),
                QuickAccessFolderNode n
                    => new QuickAccessFolderNodeAccessor(model, n),
                QuickAccessNode n
                    => new QuickAccessNodeAccessor(model, n),
                RootDirectoryNode n
                    => new DirectoryNodeAccessor(model, n),
                DirectoryNode n
                    => new DirectoryNodeAccessor(model, n),
                RootBookmarkFolderNode n
                    => new BookmarkFolderNodeAccessor(model, n),
                BookmarkFolderNode n
                    => new BookmarkFolderNodeAccessor(model, n),
                DummyNode n
                    => new NodeAccessor(model, n),
                _
                    => throw new NotSupportedException($"Not support yet: {node.GetType().FullName}"),
            };
        }
    }
}
