//#define LOCAL_DEBUG

using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class BookshelfExcludeFilter
    {
        public static bool IsMatch(string s)
        {
            var excludeRegexs = Config.Current.Bookshelf.GetExcludeRegexes();
            return excludeRegexs.Count > 0 && excludeRegexs.Any(r => r.IsMatch(s));
        }

        public static List<FolderItem> Filter(List<FolderItem> items)
        {
            var ecludeRegexs = Config.Current.Bookshelf.GetExcludeRegexes();
            if (ecludeRegexs.Count > 0)
            {
                items = items
                    .Where(e => e.Name != null && !ecludeRegexs.Any(r => r.IsMatch(e.Name)))
                    .ToList();
            }
            return items;
        }

    }
}
