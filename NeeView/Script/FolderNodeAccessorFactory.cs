using NeeView.Collections.Generic;
using System;

namespace NeeView
{
    public static class FolderNodeAccessorFactory
    {
        public static NodeAccessor Create(FolderTreeModel model, ITreeViewNode node)
        {
            return node switch
            {
                TreeListNode<QuickAccessEntry> { Value: QuickAccessFolder } n
                    => new QuickAccessFolderNodeAccessor(model, n),
                TreeListNode<QuickAccessEntry> { Value: QuickAccess } n
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
