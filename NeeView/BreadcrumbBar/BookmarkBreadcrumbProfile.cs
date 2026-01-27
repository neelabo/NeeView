using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public class BookmarkBreadcrumbProfile : IBreadcrumbProfile
    {
        public string GetDisplayName(QueryPath query, int index)
        {
            var s = query.Tokens[index];
            if (index == 0)
            {
                Debug.Assert(s == QueryScheme.Bookmark.ToSchemeString());
                return QueryScheme.Bookmark.ToAliasName();
            }
            return s;
        }

        public async ValueTask<List<BreadcrumbToken>> GetChildrenAsync(QueryPath query, CancellationToken token)
        {
            if (query.Scheme != QueryScheme.Bookmark) return new();

            var node = BookmarkCollection.Current.FindNode(query);
            if (node is null) return new();

            var list = node
                .Select(e => e.Value)
                .OfType<BookmarkFolder>()
                .Select(e => e.Name)
                .WhereNotNull()
                .Select(e => new BreadcrumbToken(query, e, null))
                .ToList();

            await ValueTask.CompletedTask;
            return list;
        }

        public bool CanHasChild(QueryPath query)
        {
            return true;
        }
    }
}
