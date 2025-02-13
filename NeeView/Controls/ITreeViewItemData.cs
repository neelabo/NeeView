using System.Collections.Generic;

namespace NeeView
{
    public interface ITreeViewItemData
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        IEnumerable<ITreeViewItemData>? Children { get; }
    }


    public interface ITreeViewItemData<T>
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        IEnumerable<ITreeViewItemData<T>>? Children { get; }
    }


    public static class TreeViewItemDataExtensions
    {
        public static IEnumerable<ITreeViewItemData> WalkChildren(this ITreeViewItemData node)
        {
            if (!node.IsExpanded || node.Children is null)
            {
                yield break;
            }

            foreach (var child in node.Children)
            {
                yield return child;
                foreach (var subChild in WalkChildren(child))
                {
                    yield return subChild;
                }
            }
        }

        public static IEnumerable<ITreeViewItemData> Walk(this ITreeViewItemData node)
        {
            yield return node;

            foreach (var child in WalkChildren(node))
            {
                yield return child;
            }
        }

        public static IEnumerable<ITreeViewItemData<T>> WalkChildren<T>(this ITreeViewItemData<T> item)
        {
            if (!item.IsExpanded || item.Children is null)
            {
                yield break;
            }

            foreach (var child in item.Children)
            {
                yield return child;
                foreach (var subChild in WalkChildren(child))
                {
                    yield return subChild;
                }
            }
        }

        public static IEnumerable<ITreeViewItemData<T>> Walk<T>(this ITreeViewItemData<T> item)
        {
            yield return item;

            foreach (var child in WalkChildren(item))
            {
                yield return child;
            }
        }
    }
}
