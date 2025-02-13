using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// [Immutable]
    /// 指向性ページ範囲
    /// </summary>
    public struct PageRange : IEquatable<PageRange>, IComparable<PageRange>
    {
        public PageRange()
        {
            Min = new PagePosition();
            Max = Min;
        }

        public PageRange(PagePosition position, int partSize)
        {
            if (position.IsEmpty() || partSize == 0)
            {
                Min = PagePosition.Empty;
                Max = Min;
                return;
            }

            if (partSize >= 0)
            {
                Min = position;
                Max = position + (partSize - 1);
            }
            else
            {
                Min = position + (partSize + 1);
                Max = position;
            }
        }

        public PageRange(PagePosition p0, PagePosition p1)
            : this(new[] { p0, p1 })
        {
        }

        public PageRange(IEnumerable<PagePosition> positions)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));

            var list = positions.Where(e => !e.IsEmpty());

            if (!list.Any())
            {
                Min = PagePosition.Empty;
                Max = Min;
                return;
            }

            Min = list.Min();
            Max = list.Max();
        }

        public PageRange(IEnumerable<PageRange> ranges)
            : this(PageRangesToPositions(ranges))
        {
        }


        public static readonly PageRange Empty = new(PagePosition.Empty, PagePosition.Empty);

        /// <summary>
        /// 範囲開始
        /// </summary>
        public PagePosition Min { get; }

        /// <summary>
        /// 範囲終了
        /// </summary>
        public PagePosition Max { get; }

        /// <summary>
        /// パーツサイズ
        /// </summary>
        public int PartSize => System.Math.Abs(Max.Value - Min.Value) + 1;

        /// <summary>
        /// ページサイズ
        /// </summary>
        public int PageSize => System.Math.Abs(Max.Index - Min.Index) + 1;


        /// <summary>
        /// １ページ分のPageRangeを作成する
        /// </summary>
        /// <param name="position">ページ開始位置</param>
        /// <param name="direction">ページとして有効な方向(+1/-1)</param>
        public static PageRange CreatePageRangeForOnePage(PagePosition position, int direction)
        {
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var last = new PagePosition(position.Index, direction > 0 ? 1 : 0);
            return new PageRange(position, last);
        }

        private static List<PagePosition> PageRangesToPositions(IEnumerable<PageRange> parts)
        {
            return parts.Where(e => !e.IsEmpty()).SelectMany(e => e.CollectTerminals()).ToList();
        }


        public string ToDisplayString()
        {
            if (Min.Part == 0)
            {
                var postfix = PartSize >= 2 ? "ab" : "a";
                return Min.Index.ToString(CultureInfo.InvariantCulture) + postfix;
            }
            else
            {
                var postfix = PartSize >= 2 ? "b+" : "b";
                return Min.Index.ToString(CultureInfo.InvariantCulture) + postfix;
            }
        }

        public override string ToString()
        {
            return $"{Min}+{Max}";
        }

        public bool IsEmpty()
        {
            return Min.IsEmpty() || Max.IsEmpty();
        }

        public bool IsOnePage()
        {
            return Min.Index == Max.Index;
        }

        public bool Contains(PagePosition position)
        {
            if (position.IsEmpty())
            {
                return false;
            }

            return Min <= position && position <= Max;
        }

        public bool Contains(PageRange range)
        {
            if (range.IsEmpty())
            {
                return false;
            }

            return Contains(range.Min) && Contains(range.Max);
        }

        public bool Conflict(PageRange other)
        {
            return Min <= other.Max && other.Min <= Max;
        }

        public PageRange Add(PagePosition position)
        {
            // TODO: コレクションリテラルの使用 (C#12)
            return new PageRange(new[] { Min, Max, position });
        }

        public PageRange Add(PageRange other)
        {
            // TODO: コレクションリテラルの使用 (C#12)
            return new PageRange(new[] { Min, Max, other.Min, other.Max });
        }

        public PageRange Add(IEnumerable<PageRange> others)
        {
            return new PageRange(others.Append(this));
        }

        public PagePosition Next()
        {
            return Next(+1);
        }

        public PagePosition Previous()
        {
            return Next(-1);
        }

        /// <summary>
        /// PageRangeの次の位置を求める
        /// </summary>
        /// <param name="direction">方向</param>
        public PagePosition Next(int direction)
        {
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            return direction < 0 ? Min - 1 : Max + 1;
        }

        /// <summary>
        /// PageRangeの基準位置を求める
        /// </summary>
        /// <param name="direction">方向</param>
        public PagePosition Top(int direction)
        {
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var pos = direction < 0 ? Max : Min;
            return pos;
        }

        /// <summary>
        /// パーツ指定を外した、リソースベースのページ範囲を求める
        /// TODO: 必要？PartRangeの機能範囲外である。
        /// </summary>
        public PageRange Truncate()
        {
            var min = new PagePosition(Min.Index, 0);
            var max = new PagePosition(Max.Index, 0);
            var range = new PageRange(min, max);
            return range;
        }

        /// <summary>
        /// 範囲を制限する
        /// </summary>
        public PageRange Clamp(PagePosition p0, PagePosition p1)
        {
            var minLimit = p0 < p1 ? p0 : p1;
            var maxLimit = p0 < p1 ? p1 : p0;

            var min = Min < minLimit ? minLimit : Min;
            var max = Max > maxLimit ? maxLimit : Max;

            return new PageRange(min, max);
        }

        /// <summary>
        /// 範囲をマージする
        /// </summary>
        public static PageRange Marge(IEnumerable<PageRange> pageRanges)
        {
            return new PageRange(pageRanges);
        }


        public IEnumerable<PagePosition> CollectPositions()
        {
            if (IsEmpty()) yield break;

            for (int i = 0; i < PartSize; i++)
            {
                yield return Min + i;
            }
        }

        public IEnumerable<PagePosition> CollectTerminals()
        {
            if (IsEmpty()) yield break;

            yield return Min;
            yield return Max;
        }

        public override bool Equals(object? obj)
        {
            return obj is PageRange range && Equals(range);
        }

        public bool Equals(PageRange other)
        {
            return Min.Equals(other.Min) &&
                   PartSize == other.PartSize;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, PartSize);
        }

        // TODO: この比較方法でよいの？
        public int CompareTo(PageRange other)
        {
            var result = Min.CompareTo(other.Min);
            if (result == 0)
            {
                result = PartSize.CompareTo(other.PartSize);
            }
            return result;
        }

        public int CompareConflictTo(PageRange other)
        {
            // 範囲が被っているときは０を返す
            var minCompare = Math.Clamp(Min.CompareTo(other.Min), -1, +1);
            var maxCompare = Math.Clamp(Max.CompareTo(other.Max), -1, +1);
            return minCompare == maxCompare ? minCompare : 0;
        }


        public static bool operator ==(PageRange left, PageRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PageRange left, PageRange right)
        {
            return !(left == right);
        }

        public static bool operator <(PageRange left, PageRange right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(PageRange left, PageRange right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(PageRange left, PageRange right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(PageRange left, PageRange right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
