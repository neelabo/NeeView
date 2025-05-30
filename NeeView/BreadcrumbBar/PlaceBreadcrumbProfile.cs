using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public class PlaceBreadcrumbProfile : IBreadcrumbProfile
    {
        private readonly AddressBreadcrumbProfile _fileBreadcrumbProfile = new() { IncludeFile = false };
        private readonly BookmarkBreadcrumbProfile _bookmarkBreadcrumbProfile = new();


        public string GetDisplayName(QueryPath query, int index)
        {
            if (query.Scheme == QueryScheme.File)
            {
                return _fileBreadcrumbProfile.GetDisplayName(query, index);
            }
            else if (query.Scheme == QueryScheme.Bookmark)
            {
                return _bookmarkBreadcrumbProfile.GetDisplayName(query, index);
            }
            else
            {
                return query.Tokens[index];
            }
        }

        public async ValueTask<List<BreadcrumbToken>> GetChildrenAsync(QueryPath query, CancellationToken token)
        {
            if (query.Scheme == QueryScheme.File)
            {
                if (string.IsNullOrEmpty(query.Path))
                {
                    return GetRootChildren();
                }
                return await _fileBreadcrumbProfile.GetChildrenAsync(query, token);
            }
            else if (query.Scheme == QueryScheme.Bookmark)
            {
                return await _bookmarkBreadcrumbProfile.GetChildrenAsync(query, token);
            }
            else
            {
                return new();
            }
        }

        private List<BreadcrumbToken> GetRootChildren()
        {
            return _fileBreadcrumbProfile.GetDriveChildren()
                .Append(new SchemeBreadcrumbToken(QueryScheme.Bookmark))
                .ToList();
        }

        public bool CanHasChild(QueryPath query)
        {
            if (string.IsNullOrEmpty(query.Path))
            {
                // Root breadcrumb is always true.
                return true;
            }
            else if (query.Scheme == QueryScheme.File)
            {
                return _fileBreadcrumbProfile.CanHasChild(query);
            }
            else if (query.Scheme == QueryScheme.Bookmark)
            {
                return _bookmarkBreadcrumbProfile.CanHasChild(query);
            }
            else
            {
                return true;
            }
        }

        public QueryPath GetQueryPath(string path)
        {
            var query = new QueryPath(path);
            if (query.Scheme == QueryScheme.Root)
            {
                query = new QueryPath("");
            }
            return query;
        }

        public List<Breadcrumb> ArrangeBreadCrumbs(List<Breadcrumb> crumbs)
        {
            if (crumbs.Count == 0 || crumbs[0].HasName)
            {
                return crumbs.Prepend(new NormalBreadcrumb(this, new QueryPath(""), 0)).ToList();
            }
            return crumbs;
        }

    }
}
