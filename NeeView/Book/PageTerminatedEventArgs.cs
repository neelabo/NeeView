using System;

namespace NeeView
{
    /// <summary>
    /// ページ終端イベントのパラメーター
    /// </summary>
    public class PageTerminatedEventArgs : EventArgs
    {
        public PageTerminatedEventArgs(int direction, bool isMedia)
        {
            Direction = direction;
            IsMedia = isMedia;
        }

        public int Direction { get; }
        public bool IsMedia { get; }
    }
}

