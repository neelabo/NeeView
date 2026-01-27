using NeeView.Runtime.LayoutPanel;
using System;

namespace NeeView
{
    /// <summary>
    /// パネルドロップイベント
    /// </summary>
    public class LayoutPanelDroppedEventArgs : EventArgs
    {
        public LayoutPanelDroppedEventArgs(LayoutPanel panel, int index)
        {
            Panel = panel;
            Index = index;
        }

        /// <summary>
        /// ドロップされたパネル
        /// </summary>
        public LayoutPanel Panel { get; set; }

        /// <summary>
        /// 挿入位置
        /// </summary>
        public int Index { get; set; }
    }
}
