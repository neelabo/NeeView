using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public interface IPageContentLoader
    {
        bool IsLoaded { get; }

        ValueTask LoadContentAsync(CancellationToken token);
    }

}
