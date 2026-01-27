using NeeView.Runtime.LayoutPanel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// AlternativePanel を元のパネルに戻すときのパラメータ
    /// </summary>
    public class AlternativePanelSource
    {
        /// <summary>
        /// AlternativePanel のキー
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// 表示されていたか
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// フローティングだったか
        /// </summary>
        public bool IsFloating { get; set; }

        /// <summary>
        /// ドッキング並び
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// ドッキングの基準パネルのキー
        /// </summary>
        public string? TargetPanel { get; set; }

        /// <summary>
        /// ドッキングのときの幅の基準パネル比率
        /// </summary>
        public double GridLengthRate { get; set; }

        /// <summary>
        /// ドッキング方向
        /// </summary>
        public int Direction { get; set; }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="panel"></param>
        /// <returns></returns>
        public static AlternativePanelSource Create(LayoutPanelManager manager, LayoutPanel panel)
        {
            var node = manager.FindLayoutDockPanelNode(panel);

            var memento = new AlternativePanelSource()
            {
                Key = panel.Key,
                IsVisible = manager.IsPanelVisible(panel),
                IsFloating = manager.IsPanelFloating(panel),
            };

            if (node is not null)
            {
                memento.Orientation = node.Panels.Orientation;
                var index = node.Panels.IndexOf(panel);
                var direction = index > 0 ? +1 : -1;
                var targetIndex = index - direction;
                if (0 <= targetIndex && targetIndex < node.Panels.Count)
                {
                    var target = node.Panels[targetIndex];
                    memento.TargetPanel = target.Key;
                    memento.GridLengthRate = panel.GridLength.Value / target.GridLength.Value;
                    memento.Direction = direction;
                }
            }

            return memento;
        }

        /// <summary>
        /// 復元
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="panel"></param>
        public void Restore(LayoutPanelManager manager, LayoutPanel panel)
        {
            if (Key != panel.Key) return;

            if (IsFloating)
            {
                if (IsVisible)
                {
                    manager.OpenWindow(panel);
                }
            }
            else
            {
                if (TargetPanel != null && Direction != 0)
                {
                    if (!manager.Panels.TryGetValue(TargetPanel, out var target)) return;
                    var node = manager.FindLayoutDockPanelNode(target);
                    if (node is null || node.Panels.Orientation != Orientation) return;
                    var index = node.Panels.IndexOf(target);
                    Debug.Assert(0 <= index);
                    panel.GridLength = new GridLength(GridLengthRate * target.GridLength.Value, GridUnitType.Star);
                    manager.Remove(panel);
                    var offset = Direction < 0 ? 0 : 1;
                    node.Panels.Insert(index + offset, panel);
                }
                if (IsVisible)
                {
                    var node = manager.FindLayoutDockPanelNode(panel);
                    if (node != null && node.Dock.SelectedItem is null)
                    {
                        node.Dock.SelectedItem = node.Panels;
                    }
                }
            }
        }
    }
}
