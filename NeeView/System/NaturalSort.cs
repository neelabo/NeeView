using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// 自然順ソート
    /// </summary>
    public static class NaturalSort
    {
        private static readonly IComparer<string> _asciiComparer;
        private static readonly IComparer<string> _nativeComparer;
        private static readonly IComparer<string> _naturalComparer;

        static NaturalSort()
        {
            _asciiComparer = StringComparer.Ordinal;
            _nativeComparer = new NativeNaturalComparer();
            _naturalComparer = new NaturalComparer();
        }

        public static IComparer<string> Comparer => Config.Current.System.StringComparerType switch
        {
            StringComparerType.Ascii => _asciiComparer,
            StringComparerType.Native => _nativeComparer,
            StringComparerType.Natural => _naturalComparer,
            _ => _nativeComparer
        };

        public static int Compare(string? x, string? y)
        {
            return Comparer.Compare(x, y);
        }
    }

}
