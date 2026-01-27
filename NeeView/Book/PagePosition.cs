
using System;
using System.Globalization;


namespace NeeView
{
    /// <summary>
    /// ページの場所を表す構造体。
    /// ページ番号と、部分を示すパーツ番号で構成されています。
    /// </summary>
    public struct PagePosition : IComparable<PagePosition>, IEquatable<PagePosition>
    {
        private readonly int _value;


        public PagePosition(int value)
        {
            _value = value;
        }

        public PagePosition(int index, int part)
        {
            _value = index * 2 + part;
        }


        /// <summary>
        /// ページの場所(0,0)
        /// </summary>
        public static PagePosition Zero { get; } = new PagePosition(0);

        /// <summary>
        /// ページの場所：無効
        /// </summary>
        /// <remarks>
        /// 厳密にはすべての値が有効なのでこの Empty は不正確。NeeViewにおいてはこの場所に到達することはほぼないため機能している。
        /// </remarks>
        public static PagePosition Empty { get; } = new PagePosition(int.MinValue);

        //
        public int Value => _value;

        // ページ番号
        public int Index => (_value >= 0) ? _value / 2 : (_value - 1) / 2;

        // パーツ番号
        public int Part => (_value >= 0) ? _value % 2 : (-_value % 2);


        //
        public override string ToString()
        {
            return IsEmpty() ? "Empty" : Index.ToString(CultureInfo.InvariantCulture) + (Part == 1 ? "b" : "a");
        }

        // truncate ... パーツ番号クリア
        public PagePosition Truncate()
        {
            return new PagePosition(Index, 0);
        }

        // もう一方のパーツの場所
        public PagePosition OtherPart()
        {
            return new PagePosition(Index, 1 - Part);
        }

        // clamp
        public PagePosition Clamp(PagePosition min, PagePosition max)
        {
            if (min._value > max._value) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min");

            int value = _value;
            if (value < min._value) value = min._value;
            if (value > max._value) value = max._value;

            return new PagePosition(value);
        }

        public bool IsEmpty()
        {
            return this == Empty;
        }

        // ページ数とパーツ数からValue値を求める
        public static int GetValue(int index, int part = 0)
        {
            return index * 2 + part;
        }

        // compare
        public bool Equals(PagePosition other)
        {
            return _value == other._value;
        }

        public override bool Equals(object? obj)
        {
            return obj is PagePosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public int CompareTo(PagePosition other)
        {
            return _value - other._value;
        }

        public static bool operator ==(PagePosition a, PagePosition b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(PagePosition a, PagePosition b)
        {
            return a._value != b._value;
        }

        public static bool operator <(PagePosition a, PagePosition b)
        {
            return a._value < b._value;
        }

        public static bool operator >(PagePosition a, PagePosition b)
        {
            return a._value > b._value;
        }

        public static bool operator <=(PagePosition a, PagePosition b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(PagePosition a, PagePosition b)
        {
            return a._value >= b._value;
        }

        // add
        public static PagePosition operator +(PagePosition a, PagePosition b)
        {
            return new PagePosition(a._value + b._value);
        }

        public static PagePosition operator +(PagePosition a, int b)
        {
            return new PagePosition(a._value + b);
        }

        public static PagePosition operator -(PagePosition a, PagePosition b)
        {
            return new PagePosition(a._value - b._value);
        }

        public static PagePosition operator -(PagePosition a, int b)
        {
            return new PagePosition(a._value - b);
        }
    }
}
