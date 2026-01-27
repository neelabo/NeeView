using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public class DefaultBreadcrumbProfile : IBreadcrumbProfile
    {
        public static DefaultBreadcrumbProfile Instance { get; } = new();

        public async ValueTask<List<BreadcrumbToken>> GetChildrenAsync(QueryPath query, CancellationToken token)
        {
            await ValueTask.CompletedTask;
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
