using NeeView.Runtime.LayoutPanel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    static class LayoutPanelFactory
    {
        public static List<LayoutPanel> CreatePanels(IEnumerable<IPanel> panels)
        {
            return panels.Select(e => Create(e)).ToList();
        }

        public static LayoutPanel Create(IPanel panel)
        {
            var ghost = new Border()
            {
                Width = 28,
                Height = 28,
                Child = new Image()
                {
                    Width = 28,
                    Height = 28,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Source = panel.Icon,
                }
            };
            var size = new Size(ghost.Width, ghost.Height);
            ghost.Measure(size);
            ghost.Arrange(new Rect(size));
            //ghost.UpdateLayout(); ... メインウィンドウ生成前での実行だと例外になる

            return new LayoutPanel(panel.TypeCode)
            {
                Title = panel.IconTips,
                Content = panel.View,
                DragGhost = ghost,
            };
        }
    }

}
