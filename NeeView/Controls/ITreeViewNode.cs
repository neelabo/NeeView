using System.Collections.Generic;

namespace NeeView
{
    public interface ITreeViewNode : ITreeNode
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }


    public interface ITreeViewNode<T> : ITreeNode<T>
        where T : ITreeViewNode<T>, ITreeNode<T>
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }


    public static class TreeViewNodeExtensions
    {
        public static IEnumerable<ITreeViewNode> WalkChildren(this ITreeViewNode node)
        {
            if (!node.IsExpanded || node.Children is null)
            {
                yield break;
            }

            foreach (ITreeViewNode child in node.Children)
            {
                yield return child;
                foreach (var subChild in WalkChildren(child))
                {
                    yield return subChild;
                }
            }
        }

        public static IEnumerable<ITreeViewNode> Walk(this ITreeViewNode node)
        {
            yield return node;

            foreach (var child in WalkChildren(node))
            {
                yield return child;
            }
        }

        public static IEnumerable<T> WalkChildren<T>(this T item)
            where T : ITreeViewNode<T>
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

        public static IEnumerable<T> Walk<T>(this T item)
            where T : ITreeViewNode<T>
        {
            yield return item;

            foreach (var child in WalkChildren(item))
            {
                yield return child;
            }
        }
    }
}
