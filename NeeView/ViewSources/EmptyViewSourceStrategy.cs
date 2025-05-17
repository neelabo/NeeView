using NeeView.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class EmptyViewSourceStrategy : IViewSourceStrategy
    {
        public EmptyViewSourceStrategy()
        {
        }

        public async ValueTask<DataSource> LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            return await Task.FromResult(new DataSource(new EmptyViewData(), 0, null));
        }
    }


    public class EmptyViewData
    {
    }
}
