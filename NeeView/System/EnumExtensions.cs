using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 文字列からEnumに変換
        /// </summary>
        /// <param name="s">文字列</param>
        /// <param name="type">enum型</param>
        /// <returns>Enum値。変換できなかった場合は default を返す。</returns>
        /// <exception cref="ArgumentException">type が enum 型ではない</exception>
        public static object ToEnum(this string s, Type type)
        {
            if (type is null || !type.IsEnum) throw new ArgumentException("type must be enum.");

            try
            {
                return Enum.Parse(type, s);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Requested {type.Name} value '{s}' was not found.", ex);
            }
        }

        /// <summary>
        /// 文字列からEnumに変換 (Fuzzy)
        /// </summary>
        /// <param name="s">文字列</param>
        /// <param name="type">enum型</param>
        /// <returns>Enum値</returns>
        /// <exception cref="ArgumentException">type が enum 型ではない</exception>
        /// <exception cref="InvalidCastException">変換失敗</exception>
        public static object ToEnumOrDefault(this string s, Type type)
        {
            if (type is null || !type.IsEnum) throw new ArgumentException("type must be enum.");

            if (Enum.TryParse(type, s, out var result))
            {
                return result ?? throw new InvalidOperationException();
            }
            else
            { 
                return Enum.GetValues(type).GetValue(0) ?? throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 文字列からEnumに変換
        /// </summary>
        /// <returns>
        /// Enum値
        /// </returns>
        /// <exception cref="System.InvalidCastException">変換失敗</exception>
        public static TEnum ToEnum<TEnum>(this string s)
            where TEnum : struct, Enum
        {
            return Enum.TryParse(s, out TEnum result) ? result : throw new InvalidCastException($"Requested {typeof(TEnum).Name} enum value '{s}' was not found.");
        }

        /// <summary>
        /// 文字列からEnumに変換 (Fuzzy)
        /// </summary>
        /// <returns>
        /// Enum値。変換できなかった場合は default を返す。
        /// </returns>
        public static TEnum ToEnumOrDefault<TEnum>(this string s)
            where TEnum : struct, Enum
        {
            return Enum.TryParse(s, out TEnum result) ? result : Enum.GetValues<TEnum>()[0];
        }
    }
}

