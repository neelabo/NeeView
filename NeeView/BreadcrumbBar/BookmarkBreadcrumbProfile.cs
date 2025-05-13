using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NeeView
{
    public class BookmarkBreadcrumbProfile : IBreadcrumbProfile
    {
        public string GetDisplayName(string s, int index)
        {
            if (index == 0 && s == QueryScheme.Bookmark.ToSchemeString())
            {
                return QueryScheme.Bookmark.ToAliasName();
            }
            else
            {
                return s;
            }
        }

        public List<string> GetDirectories(string path)
        {
            try
            {
                var query = new QueryPath(path);
                if (query.Scheme != QueryScheme.Bookmark) return new();

                var node = BookmarkCollection.Current.FindNode(query);
                if (node is null) return new();

                var list = node.Children.Select(e => e.Value).OfType<BookmarkFolder>().Select(e => e.Name).WhereNotNull().ToList();
                return list;
            }
            catch
            {
                return new();
            }
        }
    }

}
