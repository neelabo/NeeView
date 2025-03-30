using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView.Setting
{
    public class ContextMenuSettingViewModel : BindableBase
    {
        private MenuTree? _root;
        private ContextMenuSetting? _contextMenuSetting;
        private List<MenuTree> _sourceElementList;
        private int _commandTableChangeCount;
        private int _selectedElementIndex;


        public ContextMenuSettingViewModel()
        {
            if (CommandTable.Current == null) throw new InvalidOperationException();

            _commandTableChangeCount = CommandTable.Current.ChangeCount;
            _sourceElementList = CreateSourceElementList();
            _selectedElementIndex = 0;
        }


        public MenuTree? Root
        {
            get { return _root; }
            set { SetProperty(ref _root, value); }
        }


        public List<MenuTree> SourceElementList
        {
            get { return _sourceElementList; }
            private set { SetProperty(ref _sourceElementList, value); }
        }


        public int SelectedElementIndex
        {
            get { return _selectedElementIndex; }
            set { SetProperty(ref _selectedElementIndex, value); }
        }


        public void UpdateSource()
        {
            if (_commandTableChangeCount != CommandTable.Current.ChangeCount)
            {
                _commandTableChangeCount = CommandTable.Current.ChangeCount;
                this.SourceElementList = CreateSourceElementList();
                this.SelectedElementIndex = 0;

                this.Root?.RaisePropertyChangedAll();
            }
        }

        private static List<MenuTree> CreateSourceElementList()
        {
            var list = CommandTable.Current.Values
            .OrderBy(e => e.Order)
            .GroupBy(e => e.Group)
            .SelectMany(g => g)
            .Select(e => new MenuTree() { MenuElementType = MenuElementType.Command, CommandName = e.Name })
            .ToList();

            list.Insert(0, new MenuTree() { MenuElementType = MenuElementType.Group });
            list.Insert(1, new MenuTree() { MenuElementType = MenuElementType.Separator });
            list.Insert(2, new MenuTree() { MenuElementType = MenuElementType.History });

            return list;
        }

        public void Initialize(ContextMenuSetting contextMenuSetting)
        {
            _contextMenuSetting = contextMenuSetting;

            Root = _contextMenuSetting.SourceTree.Clone();

            // validate
            Root.MenuElementType = MenuElementType.Group;
            Root.Validate();
        }

        public void Decide()
        {
            if (_contextMenuSetting is null || Root is null) return;

            _contextMenuSetting.SourceTree = Root.IsEqual(MenuTree.CreateDefault()) ? null : Root;
        }

        public void Reset()
        {
            Root = MenuTree.CreateDefault();

            Decide();
        }

        private static ObservableCollection<MenuTree>? GetParentCollection(ObservableCollection<MenuTree> collection, MenuTree target)
        {
            if (collection.Contains(target)) return collection;

            foreach (var children in collection.Select(e => e.Children).WhereNotNull())
            {
                var parent = GetParentCollection(children, target);
                if (parent != null) return parent;
            }

            return null;
        }

        public void AddNode(MenuTree element, MenuTree? target)
        {
            if (Root is null) return;

            if (target == null)
            {
                if (Root.Children is null) throw new InvalidOperationException();
                Root.Children.Add(element);
            }
            else if (target.Children != null && target.IsExpanded)
            {
                target.Children.Insert(0, element);
            }
            else
            {
                var parent = target.GetParent(Root);
                if (parent != null)
                {
                    if (parent.Children is null) throw new InvalidOperationException();
                    int index = parent.Children.IndexOf(target);
                    parent.Children.Insert(index + 1, element);
                }
            }

            element.IsSelected = true;
            Root.Validate();

            Decide();
        }

        public void RemoveNode(MenuTree target)
        {
            if (Root is null) return;

            var parent = target.GetParent(Root);
            if (parent != null)
            {
                if (parent.Children is null) throw new InvalidOperationException();

                var next = target.GetNext(Root, false) ?? target.GetPrev(Root);

                parent.Children.Remove(target);
                parent.Validate();

                if (next != null) next.IsSelected = true;

                Decide();
            }
        }

        public void RenameNode(MenuTree target, string name)
        {
            target.Label = name;

            Decide();
        }

        public void MoveUp(MenuTree target)
        {
            if (Root is null) return;

            var targetParent = target.GetParent(Root);
            if (targetParent?.Children is null) throw new InvalidOperationException();

            var prev = target.GetPrev(Root);
            if (prev != null && prev != Root)
            {
                var prevParent = prev.GetParent(Root);
                if (prevParent?.Children is null) throw new InvalidOperationException();

                if (targetParent == prevParent)
                {
                    int index = targetParent.Children.IndexOf(target);
                    targetParent.Children.Move(index, index - 1);
                }
                else if (targetParent == prev)
                {
                    targetParent.Children.Remove(target);
                    int index = prevParent.Children.IndexOf(prev);
                    prevParent.Children.Insert(index, target);
                }
                else
                {
                    targetParent.Children.Remove(target);
                    int index = prevParent.Children.IndexOf(prev);
                    prevParent.Children.Insert(index + 1, target);
                }

                target.IsSelected = true;
                Root.Validate();

                Decide();
            }
        }

        public void MoveDown(MenuTree target)
        {
            if (Root is null) return;

            var targetParent = target.GetParent(Root);
            if (targetParent?.Children is null) throw new InvalidOperationException();

            var next = target.GetNext(Root);
            if (next != null && next != Root)
            {
                var nextParent = next.GetParent(Root);
                if (nextParent?.Children is null) throw new InvalidOperationException();

                if (targetParent == nextParent)
                {
                    if (next.IsExpanded)
                    {
                        if (next?.Children is null) throw new InvalidOperationException();
                        targetParent.Children.Remove(target);
                        next.Children.Insert(0, target);
                    }
                    else
                    {
                        int index = targetParent.Children.IndexOf(target);
                        targetParent.Children.Move(index, index + 1);
                    }
                }
                else
                {
                    targetParent.Children.Remove(target);
                    int index = nextParent.Children.IndexOf(next);
                    nextParent.Children.Insert(index, target);
                }

                target.IsSelected = true;
                Root.Validate();

                Decide();
            }
        }
    }
}
