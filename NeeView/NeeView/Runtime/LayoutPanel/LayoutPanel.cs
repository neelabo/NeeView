using Generator.Equals;
using NeeView.Windows;
using System;
using System.Windows;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanel : IHasDragGhost
    {
        public LayoutPanel(string key)
        {
            Key = key;
        }

        public string Key { get; init; }
        public string Title { get; init; } = "(Undefined)";
        public FrameworkElement? DragGhost { get; init; }
        public required Lazy<FrameworkElement> Content { get; init; }
        public GridLength GridLength { get; set; } = new GridLength(1, GridUnitType.Star);
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;


        public override string? ToString()
        {
            return Key ?? base.ToString();
        }

        #region support IHasDragGhost

        public FrameworkElement? GetDragGhost()
        {
            return DragGhost;
        }

        #endregion support IHasDragGhost

        #region Memento

        public LayoutPanelMemento CreateMemento()
        {
            var memento = new LayoutPanelMemento();
            memento.GridLength = GridLength;
            memento.WindowPlacement = WindowPlacement;
            return memento;
        }

        public void Restore(LayoutPanelMemento memento)
        {
            if (memento == null) return;
            GridLength = memento.GridLength;
            WindowPlacement = memento.WindowPlacement;
        }

        #endregion
    }


    [Equatable]
    public partial class LayoutPanelMemento
    {
        public GridLength GridLength { get; set; } = new GridLength(1, GridUnitType.Star);
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;
    }

}
