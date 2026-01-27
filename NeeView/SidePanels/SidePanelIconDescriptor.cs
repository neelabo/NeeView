using NeeView.Runtime.LayoutPanel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    // サイドパネルアイコンのドラッグ設定
    public class SidePanelIconDescriptor : ISidePanelIconDescriptor
    {
        private readonly SidePanelFrameViewModel _vm;
        private readonly LayoutDockPanelContent _dock;

        public SidePanelIconDescriptor(SidePanelFrameViewModel vm, LayoutDockPanelContent dock)
        {
            _vm = vm;
            _dock = dock;

            _dock.AddPropertyChanged(nameof(_dock.SelectedItem),
                (s, e) => SelectedPanelChanged?.Invoke(this, EventArgs.Empty));
        }

        public event EventHandler? SelectedPanelChanged;

        public bool IsSelected(LayoutPanel panel)
        {
            return _dock.SelectedItem?.Contains(panel) ?? false;
        }

        public FrameworkElement CreateButtonContent(LayoutPanel panel)
        {
            var imageSource = _vm.MainLayoutPanelManager.PanelsSource[panel.Key].Icon;
            return new Image() { Source = imageSource };
        }

        public void DragBegin()
        {
            _vm.DragBegin(this, EventArgs.Empty);
        }

        public void DragEnd()
        {
            _vm.DragEnd(this, EventArgs.Empty);
        }

        public void ToggleLayoutPanel(LayoutPanel panel)
        {
            _vm.MainLayoutPanelManager.Toggle(panel);
        }
    }
}
