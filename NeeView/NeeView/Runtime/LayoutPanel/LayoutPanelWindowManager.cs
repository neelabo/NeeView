using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelWindowManager
    {
        private readonly LayoutPanelManager _layoutPanelManager;
        private readonly ObservableCollection<LayoutPanelWindow> _windows = new();


        public LayoutPanelWindowManager(LayoutPanelManager manager)
        {
            _layoutPanelManager = manager;
            _windows.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler? CollectionChanged;


        public LayoutPanelManager LayoutPanelManager => _layoutPanelManager;

        public Dictionary<string, string> Resources => _layoutPanelManager.Resources;

        public Window? Owner { get; set; }

        public ObservableCollection<LayoutPanelWindow> Windows => _windows;


        public LayoutPanelWindow? GetWindow(LayoutPanel panel)
        {
            return _windows.FirstOrDefault(e => e.LayoutPanel == panel);
        }

        public bool Contains(LayoutPanel panel)
        {
            return _windows.Any(e => e.LayoutPanel == panel);
        }

        public void Open(LayoutPanel panel, WindowPlacement placement)
        {
            var window = _windows.FirstOrDefault(e => e.LayoutPanel == panel);
            if (window is null)
            {
                if (placement.IsValid())
                {
                    panel.WindowPlacement = placement;
                }

                var builder = _layoutPanelManager.WindowBuilder ?? new DefaultLayoutPanelWindowBuilder();
                window = builder.CreateWindow(this, panel);
                window.Owner = Owner;
                window.Show();
                _windows.Add(window);
            }
            else
            {
                window.Activate();
                var isTopmost = window.Topmost;
                window.Topmost = true;
                window.Topmost = isTopmost;
                window.Focus();
            }
        }

        public void Close(LayoutPanel panel)
        {
            var window = _windows.FirstOrDefault(e => e.LayoutPanel == panel);
            if (window is null) return;

            window.Close();
            _windows.Remove(window);
        }

        public void CloseAll()
        {
            while (_windows.Any())
            {
                _windows.First().Close();
            }
        }

        public void Closed(LayoutPanel panel)
        {
            var window = _windows.FirstOrDefault(e => e.LayoutPanel == panel);
            if (window is null) return;

            _windows.Remove(window);
        }

        public void Snap()
        {
            foreach (var window in _windows)
            {
                window.Snap();
            }
        }


        #region Memento

        public class Memento
        {
            public List<string>? Panels { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Panels = _windows.Select(e => e.LayoutPanel).OfType<LayoutPanel>().Select(e => e.Key).ToList();
            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;

            CloseAll();

            if (memento.Panels != null)
            {
                var panels = memento.Panels.Where(e => _layoutPanelManager.Panels.ContainsKey(e)).Select(e => _layoutPanelManager.Panels[e]).ToList();
                foreach (var panel in panels)
                {
                    _layoutPanelManager.OpenWindow(panel);
                }
            }
        }

        #endregion
    }
}
