using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


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

        public List<BreadcrumbToken> GetChildren(string path, int index, CancellationToken token)
        {
            var query = new QueryPath(path);
            if (query.Scheme != QueryScheme.Bookmark) return new();

            var node = BookmarkCollection.Current.FindNode(query);
            if (node is null) return new();

            var list = node.Children
                .Select(e => e.Value)
                .OfType<BookmarkFolder>()
                .Select(e => e.Name)
                .WhereNotNull()
                .Select(e => new BreadcrumbToken(path, e, null))
                .ToList();

            return list;
        }

        public bool CanHasChild(string path, int index)
        {
            return true;
        }
    }

}
