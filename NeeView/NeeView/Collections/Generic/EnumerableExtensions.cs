using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Collections.Generic
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// ForEach
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var e in self)
            {
                action(e);
            }
        }

        /// <summary>シーケンスを指定されたサイズのチャンクに分割します.</summary>
        /// <remarks>http://devlights.hatenablog.com/entry/20121130/p2</remarks>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> self, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));
            }

            while (self.Any())
            {
                yield return self.Take(chunkSize);
                self = self.Skip(chunkSize);
            }
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        /// <remarks>https://stackoverflow.com/questions/2471588/how-to-get-index-using-linq?rq=1</remarks>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        ///<summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            if (items is IList collection)
            {
                return collection.IndexOf(item);
            }
            else
            {
                return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
            }
        }

        /// <summary>
        /// Distinct をラムダ式で
        /// </summary>
        /// <remarks>http://neue.cc/2009/08/07_184.html</remarks>
        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T?, TKey> selector)
            where TKey : notnull
        {
            return source.Distinct(new CompareSelector<T?, TKey>(selector));
        }

        /// <summary>
        /// コレクションの要素を、要素とインデックスのタプルに変換する拡張メソッド
        /// </summary>
        /// <remarks>https://www.atmarkit.co.jp/ait/articles/1702/22/news019.html</remarks>
        public static IEnumerable<(T value, int index)> ToTuples<T>(this IEnumerable<T> collection)
        {
            return collection.Select((v, i) => (v, i));
        }

        /// <summary>
        /// 重複要素存在チェック
        /// </summary>
        /// <remarks>https://blog.beachside.dev/entry/2016/06/10/210000</remarks>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool ContainsDuplicate(this IEnumerable<string> source)
        {
            return source.GroupBy(i => i).SelectMany(g => g.Skip(1)).Any();
        }
    }

    /// <summary>
    /// IEqualityComparer<T>の実装が面倒なのでセレクタ的なものはこれで賄う
    /// </summary>
    /// <remarks>http://neue.cc/2009/08/07_184.html</remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CompareSelector<T, TKey> : IEqualityComparer<T>
        where TKey : notnull
    {
        private readonly Func<T?, TKey> _selector;

        public CompareSelector(Func<T?, TKey> selector)
        {
            _selector = selector;
        }

        public bool Equals(T? x, T? y)
        {
            return _selector(x).Equals(_selector(y));
        }

        public int GetHashCode(T? obj)
        {
            return _selector(obj).GetHashCode();
        }
    }

}
