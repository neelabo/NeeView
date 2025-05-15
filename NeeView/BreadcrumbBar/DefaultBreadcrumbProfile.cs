using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace NeeView
{
    public class DefaultBreadcrumbProfile : IBreadcrumbProfile
    {
        public List<BreadcrumbToken> GetChildren(QueryPath query, CancellationToken token)
        {
            return new();
        }

        public bool CanHasChild(QueryPath query)
        {
            return true;
        }
    }
}
