using System;

// TODO: パネルからのUI操作とスクリプトからの操作の２系統がごちゃまぜになっているので整備する

namespace NeeView
{
    [Flags]
    public enum FolderTreeCategory
    {
        QuickAccess = 0x01,
        Directory = 0x02,
        BookmarkFolder = 0x04,

        All = QuickAccess | Directory | BookmarkFolder
    }
}
