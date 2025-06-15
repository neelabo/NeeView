using NeeView.Collections.Generic;
using System;

namespace NeeView
{
    /// <summary>
    /// QuickAccessNode と QuickAccessNodeFolder の基底クラス
    /// </summary>
    public abstract class QuickAccessNodeBase : FolderTreeNodeBase
    {
        public TreeListNode<IQuickAccessEntry> QuickAccessSource => (TreeListNode<IQuickAccessEntry>?)Source ?? throw new InvalidOperationException();
    }
}
