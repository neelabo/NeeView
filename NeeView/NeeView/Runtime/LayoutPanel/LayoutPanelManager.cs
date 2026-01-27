using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelManager
    {
        public LayoutPanelManager()
        {
            Windows = new LayoutPanelWindowManager(this);
        }

        public event EventHandler? DragBegin;
        public event EventHandler? DragEnd;


        public Dictionary<string, LayoutPanel> Panels { get; protected set; } = new Dictionary<string, LayoutPanel>();

        public Dictionary<string, LayoutDockPanelContent> Docks { get; protected set; } = new Dictionary<string, LayoutDockPanelContent>();

        public LayoutPanelWindowManager Windows { get; protected set; }

        public List<LayoutPanel> Separates { get; protected set; } = new();

        public Dictionary<string, string> Resources { get; private set; } = new Dictionary<string, string>()
        {
            ["Floating"] = "_Floating",
            ["Docking"] = "Doc_king",
            ["Close"] = "_Close",
        };

        public ILayoutPanelWindowBuilder? WindowBuilder { get; set; }

        public AlternativePanelSource? AlternativePanelSource { get; set; }


        public LayoutDockPanelNode? FindLayoutDockPanelNode(LayoutPanel panel)
        {
            foreach (var dock in Docks.Values)
            {
                var item = dock.FirstOrDefault(e => e.Contains(panel));
                if (item != null)
                {
                    return new LayoutDockPanelNode(dock, item);
                }
            }

            return null;
        }

        public LayoutDockPanelContent? FindPanelListCollection(LayoutPanelCollection collection)
        {
            return Docks.Values.FirstOrDefault(e => e.Contains(collection));
        }

        public void Toggle(LayoutPanel panel)
        {
            if (IsPanelSelected(panel))
            {
                Close(panel);
            }
            else
            {
                Open(panel);
            }
        }

        public bool IsPanelSelected(LayoutPanel panel)
        {
            if (Windows.Contains(panel)) return true;

            foreach (var dock in Docks.Values)
            {
                if (dock.SelectedItem?.Contains(panel) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPanelVisible(LayoutPanel panel)
        {
            if (panel is null) return false;
            if (!panel.Content.IsValueCreated) return false;

            return panel.Content.Value.IsVisible;
        }

        public bool IsPanelDock(LayoutPanel panel)
        {
            return !Windows.Contains(panel) && !IsSeparated(panel);
        }

        public bool IsPanelFloating(LayoutPanel panel)
        {
            return (panel.WindowPlacement.IsValid() || Windows.Contains(panel)) && !IsSeparated(panel);
        }

        public bool IsSeparated(LayoutPanel panel)
        {
            return Separates.Contains(panel);
        }

        public void SetSeparate(LayoutPanel panel, bool separate)
        {
            if (separate)
            {
                if (Separates.Contains(panel)) return;
                StandAlone(panel);
                Close(panel);
                Separates.Add(panel);
            }
            else
            {
                Separates.Remove(panel);
            }
        }

        public void Open(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));
            if (IsSeparated(panel)) return;

            if (panel.WindowPlacement.IsValid() || Windows.Contains(panel))
            {
                OpenWindow(panel);
            }
            else
            {
                OpenDock(panel);
            }
        }

        public void OpenWindow(LayoutPanel panel)
        {
            OpenWindow(panel, WindowPlacement.None);
        }

        public void OpenWindow(LayoutPanel panel, WindowPlacement placement)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));
            if (IsSeparated(panel)) return;

            StandAlone(panel);
            CloseDock(panel);
            Windows.Open(panel, placement);
        }

        public void OpenDock(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));
            if (IsSeparated(panel)) return;

            Windows.Close(panel);
            var node = FindLayoutDockPanelNode(panel);
            if (node == null) throw new InvalidOperationException($"This panel is not registered.: {panel}");
            node.Dock.SelectedItem = node.Panels;
        }

        public void Close(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            Windows.Close(panel);
            CloseDock(panel);
        }

        private void CloseDock(LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            foreach (var dock in Docks.Values)
            {
                if (dock.SelectedItem?.Contains(panel) == true)
                {
                    dock.SelectedItem = null;
                }
            }
        }

        public void Remove(LayoutPanel panel)
        {
            Windows.Close(panel);

            foreach (var dock in Docks.Values)
            {
                dock.RemovePanel(panel);
            }
        }


        // パネルの独立
        public void StandAlone(LayoutPanel panel)
        {
            var node = FindLayoutDockPanelNode(panel);
            if (node == null) throw new InvalidOperationException($"This panel is not registered.: {panel}");

            if (node.Panels.IsStandAlone(panel)) return;

            node.Panels.Remove(panel);
            node.Dock.Insert(node.Dock.IndexOf(node.Panels) + 1, new LayoutPanelCollection() { panel });
        }

        public void RaiseDragBegin()
        {
            DragBegin?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseDragEnd()
        {
            DragEnd?.Invoke(this, EventArgs.Empty);
        }


        #region Memento

        public class Memento
        {
            public Dictionary<string, LayoutPanel.Memento>? Panels { get; set; }

            public Dictionary<string, LayoutDockPanelContent.Memento>? Docks { get; set; }

            public LayoutPanelWindowManager.Memento? Windows { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public AlternativePanelSource? AlternativePanelSource { get; set; }
        }


        public Memento CreateMemento()
        {
            this.Windows.Snap();

            var memento = new Memento();
            memento.Panels = this.Panels.ToDictionary(e => e.Key, e => e.Value.CreateMemento());
            memento.Docks = Docks.ToDictionary(e => e.Key, e => e.Value.CreateMemento());
            memento.Windows = this.Windows.CreateMemento();
            memento.AlternativePanelSource = AlternativePanelSource;
            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;

            this.Windows.CloseAll();

            if (memento.Panels != null)
            {
                foreach (var item in memento.Panels.Where(e => Panels.ContainsKey(e.Key)))
                {
                    Panels[item.Key].Restore(item.Value);
                }
            }

            if (memento.Docks != null)
            {
                foreach (var dock in memento.Docks.Where(e => Docks.ContainsKey(e.Key)))
                {
                    Docks[dock.Key].Restore(dock.Value);
                }
            }

            // すべてのパネル登録を保証する
            var excepts = Panels.Keys.Except(Docks.Values.SelectMany(e => e.Items).SelectMany(e => e).Select(e => e.Key)).ToList();
            foreach (var except in excepts)
            {
                Docks.Last().Value.AddPanel(Panels[except]);
            }

            this.Windows.Restore(memento.Windows);

            AlternativePanelSource = memento.AlternativePanelSource;
        }

        #endregion
    }

}
