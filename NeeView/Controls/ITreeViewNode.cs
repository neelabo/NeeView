using System.Collections.Generic;

namespace NeeView
{
    public interface ITreeViewNode : ITreeNode
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        new ITreeViewNode? Parent { get; }
        new IEnumerable<ITreeViewNode>? Children { get; }
    }


    public interface ITreeViewNode<T> : ITreeNode<T>
        where T : ITreeViewNode<T>, ITreeNode<T>
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }


    public static class TreeViewNodeExtensions
    {
        public static ITreeViewNode GetRoot(this ITreeViewNode node)
        {
            return node.Parent is null ? node : node.Parent.GetRoot();
        }

        public static int IndexOf(this IEnumerable<ITreeViewNode> nodes, ITreeViewNode node)
        {
            int index = 0;
            var comparer = EqualityComparer<ITreeViewNode>.Default;
            foreach (ITreeViewNode item in nodes)
            {
                if (comparer.Equals(item, node))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

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

        public static IEnumerable<ITreeViewNode> WalkAll(this ITreeViewNode item)
        {
            yield return item;

            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    foreach (var subChild in child.WalkAll())
                    {
                        yield return subChild;
                    }
                }
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

        public static IEnumerable<T> WalkAll<T>(this T item)
            where T : ITreeViewNode<T>
        {
            yield return item;

            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    foreach (var subChild in child.WalkAll())
                    {
                        yield return subChild;
                    }
                }
            }
        }
    }
}
