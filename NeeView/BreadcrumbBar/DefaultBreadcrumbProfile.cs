using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace NeeView
{
    public class DefaultBreadcrumbProfile : IBreadcrumbProfile
    {
        public static DefaultBreadcrumbProfile Instance { get; } = new();

        public List<BreadcrumbToken> GetChildren(QueryPath query, CancellationToken token)
        {
#if DEBUG
            return new string[] { "AAA", "BBB", "CCC" }.Select(e => new BreadcrumbToken(query, e, null)).ToList();
#else
            return new();
#endif
        }

        public bool CanHasChild(QueryPath query)
        {
            return true;
        }
    }
}
