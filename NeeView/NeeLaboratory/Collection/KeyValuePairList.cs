using System;
using System.Collections.Generic;

namespace NeeLaboratory.Collection
{
    public class KeyValuePairList<TKey, TElement> : List<KeyValuePair<TKey, TElement>>
    {
        public KeyValuePairList()
        {
        }

        public KeyValuePairList(int capacity) : base(capacity)
        {
        }

        public KeyValuePairList(IEnumerable<KeyValuePair<TKey, TElement>> collection) : base(collection)
        {
        }
    }


    public static class KeyValuePairListExtensions
    {
        public static KeyValuePairList<TKey, TElement> ToKeyValuePairList<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var list = new KeyValuePairList<TKey, TElement>();
            foreach (TSource element in source)
            {
                list.Add(new KeyValuePair<TKey, TElement>(keySelector(element), elementSelector(element)));
            }
            return list;
        }

        public static KeyValuePairList<TKey, TElement> ToKeyValuePairList<TKey, TElement>(this IEnumerable<KeyValuePair<TKey, TElement>> source)
        {
            return new KeyValuePairList<TKey, TElement>(source);
        }
    }
}
