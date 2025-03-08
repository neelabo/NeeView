using System;
using System.Collections.ObjectModel;

namespace NeeView.Collections.ObjectModel
{
    public static class ObservableCollectionExtensions
    {
        public static int FindIndex<T>(this ObservableCollection<T> collection, Predicate<T> match)
            => FindIndex(collection, 0, collection.Count, match);

        public static int FindIndex<T>(this ObservableCollection<T> collection, int startIndex, Predicate<T> match)
            => FindIndex(collection, startIndex, collection.Count - startIndex, match);

        public static int FindIndex<T>(this ObservableCollection<T> collection, int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)collection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (count < 0 || startIndex > collection.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(collection[i])) return i;
            }
            return -1;
        }
    }

}
