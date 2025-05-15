using System.Collections.Generic;
using System.Threading;


namespace NeeView
{
    public interface IBreadcrumbProfile
    {
        string GetDisplayName(QueryPath query, int index) => query.Tokens[index];

        List<BreadcrumbToken> GetChildren(QueryPath query, CancellationToken token);

        bool CanHasChild(QueryPath query) => true;

        List<Breadcrumb> ArrangeBreadCrumbs(List<Breadcrumb> crumbs) => crumbs;

        QueryPath GetQueryPath(string path) => string.IsNullOrEmpty(path) ? QueryPath.None : new QueryPath(path);
    }
}
