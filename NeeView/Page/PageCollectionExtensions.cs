using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class PageCollectionExtensions
    {
        public static List<Page> PageRangeToPages(this IReadOnlyList<Page> pages, PageRange range)
        {
            var indexes = Enumerable.Range(range.Min.Index, range.Max.Index - range.Min.Index + 1);
            return indexes.Where(e => pages.IsValidIndex(e)).Select(e => pages[e]).ToList();
        }

        public static bool IsValidIndex(this IReadOnlyList<Page> pages, int index)
        {
            return 0 <= index && index < pages.Count;
        }
    }
}
