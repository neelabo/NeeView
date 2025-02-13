using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class TreeViewTextSearchCollection : ITextSearchCollection
    {
        private readonly INavigateControl _treeView;
        private readonly List<object> _collection;
        private readonly int _selectedIndex;

        public TreeViewTextSearchCollection(INavigateControl treeView, IEnumerable<ITreeViewNode> roots)
        {
            _treeView = treeView;
            _collection = new List<object>(roots.SelectMany(e => e.Walk()));
            _selectedIndex = _collection.IndexOf(_treeView.SelectedItem);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
        }

        public int ItemsCount => _collection.Count;


        public string? GetPrimaryText(int index)
        {
            return GetElement(index)?.ToString();
        }

        public object? GetElement(int index)
        {
            if (index < 0 || index >= _collection.Count) return null;
            return _collection[index];
        }

        public void NavigateToItem(int index)
        {
            var item = GetElement(index);
            if (item is null) return;

            _treeView.NavigateToItem(item);
        }
    }
}
