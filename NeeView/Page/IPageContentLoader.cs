using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IPageContentLoader
    {
        bool IsLoaded { get; }

        ValueTask LoadContentAsync(CancellationToken token);
    }

}
