using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ページ変更アクションのイベントのパラメーター
    /// </summary>
    public class PageChangeActionEventArgs : EventArgs
    {
        public PageChangeActionEventArgs(PageChangeAction action, bool isMedia)
        {
            Debug.Assert(isMedia, "The current version only supports media events.");

            Action = action;
            IsMedia = isMedia;
        }

        public PageChangeAction Action { get; }
        public bool IsMedia { get; }
    }


    public enum PageChangeAction
    {
        None,
        Move,
        Scrubbing,
        Reward,
        PlayTimeElapsed,
    }
}

