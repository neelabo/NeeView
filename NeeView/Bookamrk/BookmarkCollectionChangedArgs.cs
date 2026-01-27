using NeeView.Collections.Generic;
using System;

namespace NeeView
{
    public enum EntryCollectionChangedAction
    {
        Add,
        Remove,
        Replace,
        Move,
        Reset,
        Rename,
        Update,
    }

    public class BookmarkCollectionChangedEventArgs : EventArgs
    {
        public EntryCollectionChangedAction Action { get; set; }
        public TreeListNode<IBookmarkEntry>? Parent { get; set; }
        public TreeListNode<IBookmarkEntry>? Item { get; set; }
        public TreeListNode<IBookmarkEntry>? Target { get; set; }

        public int OldIndex { get; set; } = -1;
        public int NewIndex { get; set; } = -1;
        public string? OldName { get; set; }

        public BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction action)
        {
            Action = action;
        }

        public BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction action, TreeListNode<IBookmarkEntry>? parent, TreeListNode<IBookmarkEntry> item)
        {
            Action = action;
            Parent = parent;
            Item = item;
        }
    }
}
