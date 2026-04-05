using System;
using System.Diagnostics;

namespace NeeView
{
    [Flags]
    public enum PagePart
    {
        None = 0,
        Right = 0x01 << 0,
        Left = 0x01 << 1,
        All = Right | Left,
    }


    public static class PagePartExtensions
    {
        public static PagePart ReverseIf(this PagePart pagePart, bool condition)
        {
            return condition ? pagePart.Reverse() : pagePart;
        }

        public static PagePart Reverse(this PagePart pagePart)
        {
            return pagePart switch
            {
                PagePart.Left => PagePart.Right,
                PagePart.Right => PagePart.Left,
                _ => pagePart,
            };
        }

        public static string ToSuffix(this PagePart pagePart)
        {
            return pagePart switch
            {
                PagePart.Left => "L",
                PagePart.Right => "R",
                _ => "",
            };
        }
    }


    public static class PagePartTools
    {
        public static PagePart CreatePagePart(PageRange range, int direction)
        {
            Debug.Assert(direction is -1 or 1);
            return range.PartSize switch
            {
                1 => (range.Min.Part == 0 ? PagePart.Left : PagePart.Right).ReverseIf(direction < 0),
                2 => PagePart.All,
                _ => throw new ArgumentOutOfRangeException(nameof(range)),
            };
        }
    }

}
