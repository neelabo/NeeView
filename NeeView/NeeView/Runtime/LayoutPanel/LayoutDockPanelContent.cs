using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

namespace NeeView.Runtime.LayoutPanel
{
    public class DragDropDescriptor : IDragDropDescriptor
    {
        private readonly LayoutPanelManager _manager;

        public DragDropDescriptor(LayoutPanelManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public void DragBegin()
        {
            _manager.RaiseDragBegin();
        }

        public void DragEnd()
        {
            _manager.RaiseDragEnd();
        }
    }

    public class LayoutDockPanelContent : BindableBase
    {
        private LayoutPanelCollection? _selectedItem;
        private LayoutPanelCollection? _lastSelectedItem;

        public LayoutDockPanelContent(LayoutPanelManager manager)
        {
            LayoutPanelManager = manager;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        public event EventHandler? CollectionChanged;

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLeaderPanels();
        }

        private void UpdateLeaderPanels()
        {
            LeaderPanels = Items.Select(x => x.First()).ToList();
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public LayoutPanelManager LayoutPanelManager { get; set; }


        public ObservableCollection<LayoutPanelCollection> Items { get; } = new ObservableCollection<LayoutPanelCollection>();

        public LayoutPanelCollection? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value is not null && !IsItemDock(value))
                {
                    value = null;
                }

                if (_selectedItem != value)
                {
                    _selectedItem = value != null && Items.Contains(value) ? value : null;
                    if (_selectedItem != null)
                    {
                        _lastSelectedItem = _selectedItem;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public LayoutPanelCollection? LastSelectedItem
        {
            get { return _lastSelectedItem != null && Items.Contains(_lastSelectedItem) ? _lastSelectedItem : null; }
        }



        private List<LayoutPanel> _leaderPanels = new();
        public List<LayoutPanel> LeaderPanels
        {
            get { return _leaderPanels; }
            set { SetProperty(ref _leaderPanels, value); }
        }

        private bool IsItemDock(LayoutPanelCollection item)
        {
            return item.All(e => LayoutPanelManager.IsPanelDock(e));
        }

        private void AttachItemsChangeCallback(LayoutPanelCollection item)
        {
            item.CollectionChanged += LayoutPanelCollection_CollectionChanged;
        }

        private void DetachItemsChangeCallback(LayoutPanelCollection item)
        {
            item.CollectionChanged -= LayoutPanelCollection_CollectionChanged;
        }

        private void LayoutPanelCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLeaderPanels();
        }


        public void ToggleSelectedItem()
        {
            if (SelectedItem is null)
            {
                var lastSelectedItem = LastSelectedItem;
                if (lastSelectedItem is not null && !IsItemDock(lastSelectedItem))
                {
                    lastSelectedItem = null;
                }
                SelectedItem = lastSelectedItem ?? Items.FirstOrDefault(e => IsItemDock(e));
            }
            else
            {
                SelectedItem = null;
            }
        }


        public void Add(LayoutPanelCollection item)
        {
            if (Items.Contains(item)) return;

            Items.Add(item);
            AttachItemsChangeCallback(item);
        }

        public void Insert(int index, LayoutPanelCollection item)
        {
            Items.Insert(index, item);
            AttachItemsChangeCallback(item);
        }

        public void Clear()
        {
            SelectedItem = null;
            foreach (var item in Items)
            {
                DetachItemsChangeCallback(item);
            }
            Items.Clear();
        }


        public void Remove(LayoutPanelCollection item)
        {
            if (!Items.Contains(item)) return;

            if (item == SelectedItem)
            {
                SelectedItem = null;
            }
            Items.Remove(item);
            DetachItemsChangeCallback(item);
        }

        public void RemoveAt(int index)
        {
            var item = Items.ElementAtOrDefault(index);
            if (item != null)
            {
                Remove(item);
            }
        }

        public bool Contains(LayoutPanelCollection item)
        {
            return Items.Contains(item);
        }

        public LayoutPanelCollection? FirstOrDefault(Func<LayoutPanelCollection, bool> predicate)
        {
            return Items.FirstOrDefault(predicate);
        }

        public LayoutPanelCollection? FirstOrDefaultPanelContains(LayoutPanel panel)
        {
            return Items.FirstOrDefault(e => e.Contains(panel));
        }

        public int IndexOf(LayoutPanelCollection item)
        {
            return Items.IndexOf(item);
        }



        public void Move(int oldIndex, int newIndex)
        {
            var newIndexFixed = NeeLaboratory.MathUtility.Clamp(newIndex, 0, Items.Count - 1);

            var item = Items[oldIndex];
            Items.Remove(item);
            Items.Insert(newIndexFixed, item);
        }

        public bool ContainsPanel(LayoutPanel panel)
        {
            return Items.SelectMany(e => e).Contains(panel);
        }

        public void AddPanel(LayoutPanel panel)
        {
            if (ContainsPanel(panel)) throw new InvalidOperationException();
            Add(new LayoutPanelCollection() { panel });
        }

        public void AddPanelRange(IEnumerable<LayoutPanel> panels)
        {
            foreach (var panel in panels)
            {
                AddPanel(panel);
            }
        }

        public void RemovePanel(LayoutPanel panel)
        {
            var list = FirstOrDefault(e => e.Contains(panel));
            if (list == null) return;

            if (list.IsStandAlone(panel))
            {
                Remove(list);
            }
            else
            {
                list.Remove(panel);
            }
        }

        // パネルの配置を変更
        // 表示中のパネルに追加配置する動作
        public void MovePanelA(LayoutPanelCollection target, int index, LayoutPanel panel)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            var node = LayoutPanelManager.FindLayoutDockPanelNode(panel);
            if (node is null) return;

            if (node.Panels == target)
            {
                target.Move(target.IndexOf(panel), index);
            }
            else
            {
                node.Panels.Remove(panel);
                if (node.Panels.Count == 0)
                {
                    node.Dock.Remove(node.Panels);
                }
                target.Insert(index, panel);
            }
        }



        // パネル単位でののドック所属を変更
        // リーダーは配下を伴う。リーダーでない場合は独立
        public void MovePanel(int index, LayoutPanel panel)
        {
            if (panel is null) throw new ArgumentNullException(nameof(panel));

            var node = LayoutPanelManager.FindLayoutDockPanelNode(panel);

            if (node == null) throw new InvalidOperationException();

            if (node.Panels.First() == panel)
            {
                if (node.Dock == this)
                {
                    Move(Items.IndexOf(node.Panels), index);
                }
                else
                {
                    node.Dock.Remove(node.Panels);
                    Insert(index, node.Panels);
                }
            }
            else
            {
                node.Panels.Remove(panel);
                Insert(index, new LayoutPanelCollection() { panel });
            }
        }


        // リスト単位でのドック移動
        // ドラッグの単位はパネルなのでこちらは使われない？
        public void MovePanel(int index, LayoutPanelCollection item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var collection = LayoutPanelManager.FindPanelListCollection(item);
            if (collection == null) throw new InvalidOperationException();

            if (collection == this)
            {
                Move(Items.IndexOf(item), index);
            }
            else
            {
                collection.Remove(item);
                Insert(index, item);
            }
        }

        #region Memento

        public class Memento
        {
            public List<PanelLayout> PanelLayout { get; set; } = new();

            public string? SelectedItem { get; set; }

            // NOTE: 旧バージョンでの読み込みでエラーにさせないためにJSON出力している
            [Obsolete] // ver 40.0
            public List<List<string>> Panels { get; set; } = new();
        }

        public class PanelLayout
        {
            public PanelLayout()
            {
            }

            public PanelLayout(LayoutPanelCollection collection)
            {
                Orientation = collection.Orientation;
                Panels = collection.Select(e => e.Key).ToList();
            }

            public PanelLayout(Orientation orientation, List<string> panels)
            {
                Orientation = orientation;
                Panels = panels;
            }

            public Orientation Orientation { get; set; } = Orientation.Vertical;
            public List<string> Panels { get; set; } = new();
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelLayout = Items.Select(e => new PanelLayout(e.Orientation, e.Select(x => x.Key).ToList())).ToList();
            memento.SelectedItem = SelectedItem?.First().Key;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            Clear();

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            if (memento.Panels is not null && !memento.PanelLayout.Any())
            {
                memento.PanelLayout = memento.Panels.Select(e => new PanelLayout(Orientation.Vertical, e)).ToList();
            }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            foreach (var panelSet in memento.PanelLayout)
            {
                var collection = new LayoutPanelCollection(panelSet.Panels.Where(x => LayoutPanelManager.Panels.ContainsKey(x)).Select(x => LayoutPanelManager.Panels[x]));
                collection.Orientation = panelSet.Orientation;

                if (collection.Any())
                {
                    Add(collection);
                }
            }

            SelectedItem = Items.FirstOrDefault(e => e.Any(x => x.Key == memento.SelectedItem));
        }

        #endregion
    }


}
