using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public interface ITreeNode
    {
        ITreeNode? Parent { get; }
        IEnumerable<ITreeNode>? Children { get; }
    }


    public interface ITreeNode<T>
        where T : ITreeNode<T>
    {
        T? Parent { get; }
        IEnumerable<T>? Children { get; }
    }

    public static class TreeNodeExtensions
    {
        public static IEnumerable<ITreeNode> GetHierarchy(this ITreeNode node)
        {
            return GteHierarchyReverse(node).Reverse();
        }

        public static IEnumerable<ITreeNode> GteHierarchyReverse(this ITreeNode node)
        {
            yield return node;
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
            {
                yield return parent;
            }
        }

        public static IEnumerable<T> GetHierarchy<T>(this T node)
            where T : ITreeNode<T>
        {
            return GteHierarchyReverse(node).Reverse();
        }

        public static IEnumerable<T> GteHierarchyReverse<T>(this T node)
            where T : ITreeNode<T>
        {
            yield return node;
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
            {
                yield return parent;
            }
        }
    }

}

