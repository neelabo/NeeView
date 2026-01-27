using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NeeView
{
    public class ContextMenuSource : BindableBase
    {
        static ContextMenuSource() => Current = new ContextMenuSource();
        public static ContextMenuSource Current { get; }


        private MenuTree _menuTree;


        private ContextMenuSource()
        {
            _menuTree = MenuTree.CreateDefault();
            AttachSourceTree(_menuTree);
        }


        public event EventHandler? ContextMenuChanged;


        public MenuTree MenuTree
        {
            get { return _menuTree; }
            set
            {
                if (_menuTree != value)
                {
                    DetachSourceTree(_menuTree);
                    _menuTree = value;
                    AttachSourceTree(_menuTree);
                    RaisePropertyChanged();
                }
            }
        }

        public void AttachSourceTree(MenuTree menuTree)
        {
            if (menuTree is null) return;
            menuTree.RoutedPropertyChanged += SourceTree_RoutedPropertyChanged;
            menuTree.RoutedCollectionChanged += SourceTree_RoutedCollectionChanged;
        }

        public void DetachSourceTree(MenuTree menuTree)
        {
            if (menuTree is null) return;
            menuTree.RoutedPropertyChanged -= SourceTree_RoutedPropertyChanged;
            menuTree.RoutedCollectionChanged -= SourceTree_RoutedCollectionChanged;
        }

        private void SourceTree_RoutedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"PropertyChanged: {e.PropertyName}");
            ContextMenuChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SourceTree_RoutedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ContextMenuChanged?.Invoke(this, EventArgs.Empty);
        }

        public MenuNode? CreateContextMenuNode()
        {
            if (MenuTree.IsEqual(MenuTree.CreateDefault()))
            {
                return null;
            }

            return MenuTreeTools.CreateMenuNode(MenuTree.Root);
        }

        public List<object> CreateContextMenuItems()
        {
            return MenuTree.CreateContextMenuItems();
        }

        public void Restore(MenuNode? contextMenuNode)
        {
            if (contextMenuNode is null)
            {
                MenuTree = MenuTree.CreateDefault();
            }
            else
            {
                var node = MenuTreeTools.CreateMenuTreeNode(contextMenuNode);
                var menuTree = new MenuTree(node);
                menuTree.Validate();
                MenuTree = menuTree;
            }
        }
    }
}
