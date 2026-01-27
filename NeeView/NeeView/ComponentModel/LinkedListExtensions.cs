using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView.ComponentModel
{
    public enum LinkedListDirection : int
    {
        Previous = -1,
        Next = 1,
    }

    public static class LinkedListDirectionExtensions
    {
        public static int ToSign(this LinkedListDirection self)
        {
            return (int)self;
        }

        public static LinkedListDirection FromSign(int sign)
        {
            Debug.Assert(sign == -1 || sign == 1);
            return (LinkedListDirection)sign;
        }

        public static LinkedListDirection Reverse(this LinkedListDirection self)
        {
            return self == LinkedListDirection.Previous ? LinkedListDirection.Next : LinkedListDirection.Previous;
        }

        public static LinkedListNode<T>? GetNext<T>(this LinkedListNode<T> self, LinkedListDirection direction)
        {
            return direction switch
            {
                LinkedListDirection.Previous
                    => self.Previous,
                LinkedListDirection.Next
                    => self.Next,
                _
                    => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(LinkedListDirection))
            };
        }

        public static LinkedListNode<T>? GetPrevious<T>(this LinkedListNode<T> self, LinkedListDirection direction)
        {
            return direction switch
            {
                LinkedListDirection.Previous
                    => self.Next,
                LinkedListDirection.Next
                    => self.Previous,
                _
                    => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(LinkedListDirection))
            };
        }

        public static int OffsetOf<T>(this LinkedListNode<T> self, LinkedListNode<T> target)
        {
            if (self.List != target.List) throw new ArgumentException("They belong to different lists.", nameof(target));

            int offset = 0;
            for (var node = self; node != null; node = node.Next)
            {
                if (node == target) return offset;
                offset++;
            }

            offset = -1;
            for (var node = self.Previous; node != null; node = node.Previous)
            {
                if (node == target) return offset;
                offset--;
            }

            throw new InvalidOperationException("Don't come here.");
        }
    }

    public static class LinkedListExtensions
    {
        public static LinkedListNode<T> AddNext<T>(this LinkedList<T> self, LinkedListNode<T> node, T value, LinkedListDirection direction)
        {
            return direction switch
            {
                LinkedListDirection.Previous
                    => self.AddBefore(node, value),
                LinkedListDirection.Next
                    => self.AddAfter(node, value),
                _
                    => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(LinkedListDirection))
            };
        }


        public static void RemoveBefore<T>(this LinkedList<T> self, LinkedListNode<T>? node)
        {
            if (node is null) return;

            while (node.Previous is not null)
            {
                self.Remove(node.Previous);
            }
        }

        public static void RemoveAfter<T>(this LinkedList<T> self, LinkedListNode<T>? node)
        {
            if (node is null) return;

            while (node.Next is not null)
            {
                self.Remove(node.Next);
            }
        }

        public static void RemoveNext<T>(this LinkedList<T> self, LinkedListNode<T>? node, LinkedListDirection direction)
        {
            if (node is null) return;

            switch (direction)
            {
                case LinkedListDirection.Previous:
                    self.RemoveBefore(node);
                    break;
                case LinkedListDirection.Next:
                    self.RemoveAfter(node);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(LinkedListDirection));
            }
        }

        public static void Remove<T>(this LinkedList<T> self, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                self.Remove(value);
            }
        }

        public static void Insert<T>(this LinkedList<T> self, T value, IComparer<T> comparer)
        {
            for (var it = self.First; it != null; it = it.Next)
            {
                if (comparer.Compare(value, it.Value) < 0)
                {
                    self.AddBefore(it, value);
                    return;
                }
            }

            self.AddLast(value);
        }

        public static void Insert<T>(this LinkedList<T> self, IEnumerable<T> values, IComparer<T> comparer)
        {
            foreach (var value in values)
            {
                self.Insert(value, comparer);
            }
        }

        public static LinkedListNode<T>? FindFirstNode<T>(this LinkedList<T> self, Func<T, bool> predicate)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            for (var node = self.First; node != null; node = node.Next)
            {
                if (predicate(node.Value)) return node;
            }
            return null;
        }

        public static LinkedListNode<T>? FindLastNode<T>(this LinkedList<T> self, Func<T, bool> predicate)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            for (var node = self.Last; node != null; node = node.Previous)
            {
                if (predicate(node.Value)) return node;
            }
            return null;
        }

        public static LinkedListNode<T>? FindNode<T>(this LinkedList<T> self, Func<T, bool> predicate, int direction)
        {
            Debug.Assert(direction == -1 || direction == 1);
            return direction < 0 ? self.FindLastNode(predicate) : self.FindFirstNode(predicate);
        }


        public static IEnumerable<LinkedListNode<T>> GetNodeEnumerable<T>(this LinkedList<T> self, LinkedListDirection direction)
        {
            if (direction == LinkedListDirection.Next)
            {
                return self.GetNodeEnumerable();
            }
            else
            {
                return self.GetReverseNodeEnumerable();
            }
        }

        public static IEnumerable<LinkedListNode<T>> GetNodeEnumerable<T>(this LinkedList<T> self)
        {
            var node = self.First;
            while (node is not null)
            {
                yield return node;
                node = node.Next;
            }
        }

        public static IEnumerable<LinkedListNode<T>> GetReverseNodeEnumerable<T>(this LinkedList<T> self)
        {
            var node = self.Last;
            while (node is not null)
            {
                yield return node;
                node = node.Previous;
            }
        }
    }
}
