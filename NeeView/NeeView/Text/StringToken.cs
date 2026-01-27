using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeeView.Text
{
    /// <summary>
    /// 自然ソート用。数字文字列を１つのトークンとして扱う
    /// </summary>
    public struct StringToken : IEquatable<StringToken>, IComparable<StringToken>
    {
        public StringToken(char c)
        {
            FirstChar = c;
            Chars = null;
            Nums = null;
        }

        public StringToken(List<char> chars, List<long> nums)
        {
            FirstChar = chars.Count > 0 ? chars[0] : default;
            Chars = chars;
            Nums = nums;
        }

        public StringToken(string s, List<long> nums)
        {
            FirstChar = s.Length > 0 ? s[0] : default;
            Chars = s.ToList();
            Nums = nums;
        }


        public char FirstChar { get; private set; }

        public List<char>? Chars { get; private set; }

        public List<long>? Nums { get; private set; }

        [MemberNotNullWhen(true, nameof(Nums))]
        public bool IsNumber() => Nums != null;


        public int CompareTo(StringToken other)
        {
            if (this.IsNumber() && other.IsNumber())
            {
                for (int n = 0; n < this.Nums.Count && n < other.Nums.Count; ++n)
                {
                    var numX = this.Nums[n];
                    var numY = other.Nums[n];
                    if (numX != numY)
                    {
                        return numX.CompareTo(numY);
                    }
                }

                if (this.Nums.Count != other.Nums.Count)
                {
                    return this.Nums.Count - other.Nums.Count;
                }
            }

            return CompareCharsTo(other);
        }

        public int CompareCharsTo(StringToken other)
        {
            if (this.Chars == null || other.Chars == null)
            {
                return this.FirstChar - other.FirstChar;
            }

            for (int i = 0; i < this.Chars.Count && i < other.Chars.Count; ++i)
            {
                if (this.Chars[i] != other.Chars[i])
                {
                    return this.Chars[i] - other.Chars[i];
                }
            }

            return this.Chars.Count - other.Chars.Count;
        }

        #region Support IEqutable
        // MSTest用

        public override int GetHashCode()
        {
            var charsHash = this.Chars is null ? 0 : this.Chars.GetHashCode();
            var numsHash = this.Nums is null ? 0 : this.Nums.GetHashCode();
            return this.FirstChar ^ charsHash ^ numsHash;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StringToken stringToken)
            {
                return Equals(stringToken);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(StringToken other)
        {
            return this.FirstChar == other.FirstChar &&
                SequenceEqual(this.Chars, other.Chars) &&
                SequenceEqual(this.Nums, other.Nums);

            static bool SequenceEqual<T>(IEnumerable<T>? x, IEnumerable<T>? y)
            {
                if (x == null)
                {
                    return y == null;
                }
                else if (y == null)
                {
                    return false;
                }
                else
                {
                    return x.SequenceEqual(y);
                }
            }
        }

        #endregion

        public override string ToString()
        {
            var tokens = new List<string>();

            if (FirstChar != '\0')
            {
                tokens.Add(FirstChar.ToString());
            }
            if (Chars != null)
            {
                tokens.Add("\"" + new string(Chars.ToArray()) + "\"");
            }
            if (Nums != null)
            {
                tokens.Add("{" + string.Join(",", Nums) + "}");
            }
            return string.Join(",", tokens);
        }


        public static bool operator ==(StringToken left, StringToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringToken left, StringToken right)
        {
            return !(left == right);
        }

        public static bool operator <(StringToken left, StringToken right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(StringToken left, StringToken right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(StringToken left, StringToken right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(StringToken left, StringToken right)
        {
            return left.CompareTo(right) >= 0;
        }
    }

}
